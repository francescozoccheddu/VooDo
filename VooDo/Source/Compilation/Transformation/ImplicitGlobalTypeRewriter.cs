using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Compilation.Problems;
using VooDo.Runtime;
using VooDo.Utils;

using static VooDo.Compilation.Scope;

namespace VooDo.Compilation.Transformation
{

    internal static class ImplicitGlobalTypeRewriter
    {

        private sealed record Field(GlobalDefinition Definition, VariableDeclarationSyntax Syntax);

        private static void GetRuntimeSymbols(SemanticModel _semantics, out INamedTypeSymbol _variableSymbol, out INamedTypeSymbol _controllerFactorySymbol)
        {
            CSharpCompilation compilation = (CSharpCompilation) _semantics.Compilation;
            MetadataReference runtime = compilation.References.First(_r => _r.Properties.Aliases.Contains(Compiler.runtimeReferenceAlias));
            IAssemblySymbol runtimeSymbol = (IAssemblySymbol) compilation.GetAssemblyOrModuleSymbol(runtime)!;
            INamedTypeSymbol? variableSymbol = runtimeSymbol.GetTypeByMetadataName(typeof(Variable).FullName!);
            INamedTypeSymbol? controllerFactorySymbol = runtimeSymbol.GetTypeByMetadataName(typeof(IControllerFactory<>).FullName!);
            if (variableSymbol is null || controllerFactorySymbol is null)
            {
                throw new NoSemanticsException();
            }
            _variableSymbol = variableSymbol;
            _controllerFactorySymbol = controllerFactorySymbol;
        }

        private static ImmutableArray<ITypeSymbol> Conciliate(ITypeSymbol? _a, ImmutableArray<ITypeSymbol> _b)
        {
            IEnumerable<ITypeSymbol> candidates = _b;
            if (_a is not null)
            {
                candidates = candidates.Append(_a);
            }
            return candidates.Distinct<ITypeSymbol>(SymbolEqualityComparer.Default).ToImmutableArray();
        }

        private static ITypeSymbol? GetInitializerType(VariableDeclarationSyntax _syntax, SemanticModel _semantics)
        {
            InvocationExpressionSyntax invocation = (InvocationExpressionSyntax) _syntax.Variables.Single().Initializer!.Value;
            ExpressionSyntax argument = invocation.ArgumentList.Arguments[1].Expression;
            return _semantics.GetTypeInfo(argument).ConvertedType;
        }

        private static ImmutableArray<ITypeSymbol> GetControllerTypes(ExpressionSyntax _syntax, SemanticModel _semantics, INamedTypeSymbol _controllerFactorySymbol)
        {
            ITypeSymbol? symbol = _semantics.GetTypeInfo(_syntax).ConvertedType;
            if (symbol is INamedTypeSymbol namedSymbol)
            {
                if (_controllerFactorySymbol.Equals(namedSymbol.ConstructedFrom, SymbolEqualityComparer.Default))
                {
                    return ImmutableArray.Create(namedSymbol.TypeArguments[0]);
                }
                else
                {
                    return namedSymbol.AllInterfaces
                        .Where(_i => _i is INamedTypeSymbol ni && ni.ConstructedFrom.Equals(_controllerFactorySymbol, SymbolEqualityComparer.Default))
                        .Select(_i => _i.TypeArguments[0])
                        .ToImmutableArray();
                }
            }
            return ImmutableArray.Create<ITypeSymbol>();
        }

        private static ImmutableArray<ImmutableArray<ITypeSymbol>> InferTypes(ImmutableArray<Field> _fields, SemanticModel _semantics, INamedTypeSymbol _controllerFactorySymbol)
        {
            CompilationUnitSyntax root = (CompilationUnitSyntax) _semantics.SyntaxTree.GetRoot();
            ImmutableArray<ITypeSymbol?> initializers = _fields
                .Select(_f => GetInitializerType(_f.Syntax, _semantics))
                .ToImmutableArray();
            ImmutableDictionary<string, int> anonFieldsMap = _fields
                .SelectIndexed((_f, _i) => (field: _f, index: _i))
                .Where(_z => _z.field.Definition.Global.IsAnonymous)
                .ToImmutableDictionary(_z => _z.field.Definition.Identifier.ValueText, _z => _z.index);

            bool TryGetControllerExpression(InvocationExpressionSyntax _syntax, out ExpressionSyntax? _expression, out int _index)
            {
                SeparatedSyntaxList<ArgumentSyntax> arguments = _syntax.ArgumentList.Arguments;
                if (arguments.Count == 2)
                {
                    if (arguments[0].Expression is MemberAccessExpressionSyntax ma
                        && ma.Expression is ThisExpressionSyntax
                        && anonFieldsMap.TryGetValue(ma.Name.Identifier.ValueText, out _index))
                    {
                        _expression = arguments[1].Expression;
                        return true;
                    }
                }
                _index = -1;
                _expression = null;
                return false;
            }

            ImmutableDictionary<int, ImmutableArray<ITypeSymbol>>? controllers = root
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .SelectNonNull(_e => TryGetControllerExpression(_e, out ExpressionSyntax? expression, out int index)
                    ? (index, GetControllerTypes(expression!, _semantics, _controllerFactorySymbol))
                    : ((int index, ImmutableArray<ITypeSymbol> types)?) null)
                .ToImmutableDictionary(_e => _e!.Value.index, _e => _e!.Value.types);
            return initializers.SelectIndexed((_a, _i) => Conciliate(_a, controllers.GetValueOrDefault(_i))).ToImmutableArray();
        }

        internal static CompilationUnitSyntax Rewrite(SemanticModel _semantics, ImmutableArray<GlobalDefinition> _globals)
        {
            ImmutableDictionary<string, GlobalDefinition> globalsMap = _globals.ToImmutableDictionary(_g => _g.Identifier.ValueText);
            CompilationUnitSyntax root = (CompilationUnitSyntax) _semantics.SyntaxTree.GetRoot();
            GetRuntimeSymbols(_semantics, out INamedTypeSymbol variableSymbol, out INamedTypeSymbol controllerFactorySymbol);
            Field? ZipGlobal(FieldDeclarationSyntax _field)
            {
                SyntaxToken name = _field.Declaration.Variables.Single().Identifier;
                if (globalsMap.TryGetValue(name.ValueText, out GlobalDefinition? definition))
                {
                    ITypeSymbol symbol = _semantics.GetTypeInfo(_field.Declaration.Type).Type!;
                    if (symbol.Equals(variableSymbol, SymbolEqualityComparer.Default))
                    {
                        return new(definition, _field.Declaration);
                    }
                }
                return null;
            }
            ImmutableArray<Field> fields = root
                .DescendantNodes()
                .OfType<FieldDeclarationSyntax>()
                .SelectNonNull(ZipGlobal)
                .ToImmutableArray();
            ImmutableArray<ImmutableArray<ITypeSymbol>> candidateTypes = InferTypes(fields, _semantics, controllerFactorySymbol);
            {
                IEnumerable<GlobalDefinition>? problems = candidateTypes
                    .Zip(fields)
                    .Where(_z => _z.First.Length != 1)
                    .Select(_z => _z.Second.Definition)
            }
            ImmutableDictionary<VariableDeclarationSyntax, ITypeSymbol> map = fields.Zip(types, (_f, _t) => KeyValuePair.Create(_f.Syntax, _t!)).ToImmutableDictionary();
            VariableDeclarationSyntax Replace(VariableDeclarationSyntax _original, VariableDeclarationSyntax _updated)
            {
                ITypeSymbol symbol = map[_original];
                TypeSyntax type = SyntaxFactory.ParseTypeName(symbol.ToDisplayString());
                TypeArgumentListSyntax typeArguments = SyntaxFactoryHelper.TypeArguments(type);
                {
                    QualifiedNameSyntax name = (QualifiedNameSyntax) _updated.Type;
                    _updated = _updated.WithType(name.WithRight(SyntaxFactory.GenericName(name.Right.Identifier, typeArguments)));
                }
                {
                    InvocationExpressionSyntax initializer = (InvocationExpressionSyntax) _updated.Variables.Single().Initializer!.Value;
                    GenericNameSyntax methodName = (GenericNameSyntax) ((MemberAccessExpressionSyntax) initializer.Expression).Name;
                    _updated = _updated.ReplaceNode(methodName, methodName.WithTypeArgumentList(typeArguments));
                }
                return _updated;
            }
            return root.ReplaceNodes(fields.Select(_f => _f.Syntax), Replace);
        }

    }


}
