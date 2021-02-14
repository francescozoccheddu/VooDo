using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Compiling.Emission;
using VooDo.Problems;
using VooDo.Runtime;
using VooDo.Utils;

using static VooDo.Compiling.Emission.Scope;

namespace VooDo.Compiling.Transformation
{

    internal static class ImplicitGlobalTypeRewriter
    {

        private sealed record Field(GlobalDefinition Definition, VariableDeclarationSyntax Syntax);

        private static void GetRuntimeSymbols(SemanticModel _semantics, out INamedTypeSymbol _controllerFactorySymbol)
        {
            CSharpCompilation compilation = (CSharpCompilation) _semantics.Compilation;
            MetadataReference runtime = compilation.References.First(_r => _r.Properties.Aliases.Contains(Compilation.runtimeReferenceAlias));
            IAssemblySymbol runtimeSymbol = (IAssemblySymbol) compilation.GetAssemblyOrModuleSymbol(runtime)!;
            INamedTypeSymbol? controllerFactorySymbol = runtimeSymbol.GetTypeByMetadataName(typeof(IControllerFactory<>).FullName!);
            if (controllerFactorySymbol is null)
            {
                throw new NoSemanticsProblem().AsThrowable();
            }
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
                .Where(_z => _z.field.Definition.Prototype.Global.IsAnonymous)
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

        private static ImmutableArray<VariableDeclarationSyntax> GetFieldDeclarations(ClassDeclarationSyntax _class, ImmutableArray<GlobalDefinition> _globals)
        {
            ImmutableDictionary<string, int> globalsMap = _globals.Enumerate().ToImmutableDictionary(_e => _e.item.Identifier.ValueText, _e => _e.index);
            VariableDeclarationSyntax[] declarations = new VariableDeclarationSyntax[_globals.Length];
            foreach (VariableDeclarationSyntax declaration in _class.Members.OfType<FieldDeclarationSyntax>().Select(_f => _f.Declaration))
            {
                SyntaxToken name = declaration.Variables.Single().Identifier;
                if (globalsMap.TryGetValue(name.ValueText, out int index))
                {
                    declarations[index] = declaration;
                }
            }
            return declarations.ToImmutableArray();
        }

        internal static CompilationUnitSyntax Rewrite(SemanticModel _semantics, ImmutableArray<GlobalDefinition> _globals, Tagger _tagger)
        {
            _globals = _globals.Where(_g => _g.Prototype.Global.Type.IsVar).ToImmutableArray();
            CompilationUnitSyntax root = (CompilationUnitSyntax) _semantics.SyntaxTree.GetRoot();
            ClassDeclarationSyntax classDeclaration = root.Members.OfType<ClassDeclarationSyntax>().Single();



            throw new NotImplementedException();
        }

    }


}
