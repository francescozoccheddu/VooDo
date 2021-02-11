using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Runtime;
using VooDo.Utils;

using static VooDo.Compilation.Scope;

namespace VooDo.Compilation.Transformation
{

    internal static class ImplicitGlobalTypeRewriter
    {

        private sealed record Field(GlobalDefinition Definition, FieldDeclarationSyntax Syntax);

        private static void GetRuntimeSymbols(SemanticModel _semantics, out INamedTypeSymbol _variableSymbol, out INamedTypeSymbol _controllerFactorySymbol)
        {
            CSharpCompilation compilation = (CSharpCompilation) _semantics.Compilation;
            MetadataReference runtime = compilation.References.First(_r => _r.Properties.Aliases.Contains(Compiler.runtimeReferenceAlias));
            IAssemblySymbol runtimeSymbol = (IAssemblySymbol) compilation.GetAssemblyOrModuleSymbol(runtime)!;
            _variableSymbol = runtimeSymbol.GetTypeByMetadataName(typeof(Variable).FullName!)!;
            _controllerFactorySymbol = runtimeSymbol.GetTypeByMetadataName(typeof(IControllerFactory<>).FullName!)!;
        }

        private static ITypeSymbol? Conciliate(ITypeSymbol? _a, ITypeSymbol? _b)
        {
            if (_a == null)
            {
                return _b;
            }
            if (_b == null)
            {
                return _a;
            }
            if (_a.Equals(_b, SymbolEqualityComparer.Default))
            {
                return _a;
            }
            return null;
        }

        private static ITypeSymbol? GetInitializerType(FieldDeclarationSyntax _syntax, SemanticModel _semantics)
        {
            InvocationExpressionSyntax invocation = (InvocationExpressionSyntax) _syntax.Declaration.Variables.Single().Initializer!.Value;
            ExpressionSyntax argument = invocation.ArgumentList.Arguments[1].Expression;
            return _semantics.GetTypeInfo(argument).ConvertedType;
        }

        private static ITypeSymbol? GetControllerType(ExpressionSyntax _syntax, SemanticModel _semantics, INamedTypeSymbol _controllerFactorySymbol)
        {
            ITypeSymbol? symbol = _semantics.GetTypeInfo(_syntax).ConvertedType;
            if (symbol is INamedTypeSymbol namedSymbol)
            {
                if (_controllerFactorySymbol.Equals(namedSymbol.ConstructedFrom, SymbolEqualityComparer.Default))
                {
                    return namedSymbol.TypeArguments[0];
                }
                ImmutableArray<INamedTypeSymbol> implementations = namedSymbol.AllInterfaces
                    .Where(_i => _i is INamedTypeSymbol ni && ni.ConstructedFrom.Equals(_controllerFactorySymbol, SymbolEqualityComparer.Default))
                    .ToImmutableArray();
                if (implementations.Length == 1)
                {
                    return implementations[0].TypeArguments[0];
                }
            }
            return null;
        }

        private static ImmutableArray<ITypeSymbol?> InferTypes(ImmutableArray<Field> _fields, SemanticModel _semantics, INamedTypeSymbol _controllerFactorySymbol)
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

            ImmutableDictionary<int, ITypeSymbol>? controllers = root
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .SelectNonNull(_e => TryGetControllerExpression(_e, out ExpressionSyntax? expression, out int index)
                    ? (index, GetControllerType(expression!, _semantics, _controllerFactorySymbol))
                    : ((int index, ITypeSymbol? symbol)?) null)
                .ToImmutableDictionary(_e => _e!.Value.index, _e => _e!.Value.symbol);
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
                        return new(definition, _field);
                    }
                }
                return null;
            }
            ImmutableArray<Field> fields = root
                .DescendantNodes()
                .OfType<FieldDeclarationSyntax>()
                .SelectNonNull(ZipGlobal)
                .ToImmutableArray();
            ImmutableArray<ITypeSymbol?> types = InferTypes(fields, _semantics, controllerFactorySymbol);
            {
                int firstError = types.IndexOf(null);
                if (firstError > 0)
                {
                    throw new ApplicationException($"Cannot infer type for global '{_globals[firstError].Global.Name ?? "<unnamed-global>"}'");
                }
            }
            throw new Exception();

        }

    }


}
