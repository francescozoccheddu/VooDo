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

        private readonly struct GlobalSyntax
        {

            public VariableDeclarationSyntax Declaration { get; }
            public ExpressionSyntax InitialValue { get; }
            public ExpressionSyntax? Controller { get; }

            public GlobalSyntax(VariableDeclarationSyntax _declaration, ExpressionSyntax _initialValue, ExpressionSyntax? _controller)
            {
                Declaration = _declaration;
                InitialValue = _initialValue;
                Controller = _controller;
            }

        }

        private static INamedTypeSymbol GetControllerFactorySymbol(SemanticModel _semantics, MetadataReference _runtimeReference)
        {
            CSharpCompilation compilation = (CSharpCompilation)_semantics.Compilation;
            IAssemblySymbol runtimeSymbol = (IAssemblySymbol)compilation.GetAssemblyOrModuleSymbol(_runtimeReference)!;
            INamedTypeSymbol? controllerFactorySymbol = runtimeSymbol.GetTypeByMetadataName(typeof(IControllerFactory<>).FullName!);
            if (controllerFactorySymbol is null)
            {
                throw new NoSemanticsProblem().AsThrowable();
            }
            return controllerFactorySymbol;
        }

        private static ITypeSymbol? GetExpressionType(ExpressionSyntax? _syntax, SemanticModel _semantics)
        {
            if (_syntax is null)
            {
                return null;
            }
            ITypeSymbol? type = _semantics.GetTypeInfo(_syntax).Type;
            if (type is null || type.TypeKind == TypeKind.Error)
            {
                return null;
            }
            return type;
        }

        private static ImmutableArray<GlobalSyntax> GetSyntax(ClassDeclarationSyntax _class, IEnumerable<GlobalDefinition> _globals, Tagger _tagger)
        {
            ImmutableDictionary<Tagger.Tag, int> globalsMap = _globals
                .Enumerate()
                .ToImmutableDictionary(_e => _e.item.Prototype.Source.GetTag(_tagger)!, _e => _e.index);
            GlobalSyntax[] syntax = new GlobalSyntax[globalsMap.Count];
            foreach (VariableDeclarationSyntax declaration in _class.Members.OfType<FieldDeclarationSyntax>().Select(_f => _f.Declaration))
            {
                Tagger.Tag? tag = declaration.GetTag();
                if (tag is not null && globalsMap.TryGetValue(tag, out int index))
                {
                    InvocationExpressionSyntax invocation = (InvocationExpressionSyntax)declaration.Variables.Single().Initializer!.Value;
                    ExpressionSyntax initialValue = invocation.ArgumentList.Arguments[2].Expression;
                    syntax[index] = new GlobalSyntax(declaration, initialValue, null);
                }
            }
            ExpressionSyntax?[] controllers = new ExpressionSyntax?[globalsMap.Count];
            MethodDeclarationSyntax method = _class.Members
                .OfType<MethodDeclarationSyntax>()
                .Where(_m => _m.Identifier.ValueText is RuntimeHelpers.runMethodName or RuntimeHelpers.typedRunMethodName && _m.Modifiers.Any(_d => _d.IsKind(SyntaxKind.OverrideKeyword)))
                .Single();
            foreach (InvocationExpressionSyntax invocation in method.Body!.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (globalsMap.TryGetValue(invocation.GetTag()!, out int index))
                {
                    ExpressionSyntax? controller = invocation.ArgumentList.Arguments[1].Expression;
                    syntax[index] = new GlobalSyntax(syntax[index].Declaration, syntax[index].InitialValue, controller);
                }
            }
            return syntax.ToImmutableArray();
        }

        private static ImmutableArray<ITypeSymbol> GetVariableTypesFromControllerType(ITypeSymbol _controller, INamedTypeSymbol _controllerFactory)
        {
            if (_controller is INamedTypeSymbol type)
            {
                return type.AllInterfaces
                    .Append(type)
                    .Where(_i => _i.ConstructedFrom.Equals(_controllerFactory, SymbolEqualityComparer.Default))
                    .Select(_i => _i.TypeArguments.Single())
                    .Distinct<ITypeSymbol>(SymbolEqualityComparer.Default)
                    .ToImmutableArray();
            }
            return ImmutableArray.Create<ITypeSymbol>();
        }

        private static ImmutableArray<ImmutableArray<ITypeSymbol>> GetControllerTypes(IEnumerable<ExpressionSyntax?> _controllers, SemanticModel _semantics, MetadataReference _runtimeReference)
        {
            INamedTypeSymbol controllerFactorySymbol = GetControllerFactorySymbol(_semantics, _runtimeReference);
            return _controllers
                .Select(_c => GetExpressionType(_c, _semantics))
                .Select(_t => _t is null
                    ? ImmutableArray.Create<ITypeSymbol>()
                    : GetVariableTypesFromControllerType(_t, controllerFactorySymbol))
                .ToImmutableArray();
        }

        private static ImmutableArray<ITypeSymbol?> GetInitialValueTypes(IEnumerable<ExpressionSyntax?> _initialValue, SemanticModel _semantics)
            => _initialValue
                .Select(_c => GetExpressionType(_c, _semantics))
                .ToImmutableArray();

        private static ImmutableArray<ITypeSymbol> ConciliateTypes(ITypeSymbol? _initialValue, ImmutableArray<ITypeSymbol> _controllerValues)
        {
            if (_initialValue is null)
            {
                return _controllerValues;
            }
            if (_controllerValues.IsDefaultOrEmpty || _controllerValues.Contains(_initialValue, SymbolEqualityComparer.Default))
            {
                return ImmutableArray.Create(_initialValue);
            }
            return ImmutableArray.Create<ITypeSymbol>();
        }

        private static ImmutableArray<ImmutableArray<ITypeSymbol>> InferTypes(IEnumerable<GlobalSyntax> _syntax, SemanticModel _semantics, MetadataReference _runtimeReference)
        {
            ImmutableArray<ImmutableArray<ITypeSymbol>> controllerTypes = GetControllerTypes(_syntax.Select(_c => _c.Controller), _semantics, _runtimeReference);
            ImmutableArray<ITypeSymbol?> initialValueTypes = GetInitialValueTypes(_syntax.Select(_iv => _iv.InitialValue), _semantics);
            return initialValueTypes.Zip(controllerTypes, (_iv, _c) => ConciliateTypes(_iv, _c)).ToImmutableArray();
        }

        private static ImmutableArray<ITypeSymbol> InferSingleType(IEnumerable<GlobalSyntax> _syntax, IEnumerable<GlobalPrototype> _prototypes, SemanticModel _semantics, MetadataReference _runtimeReference)
        {
            ImmutableArray<ImmutableArray<ITypeSymbol>> types = InferTypes(_syntax, _semantics, _runtimeReference);
            types.Zip(_prototypes, (_t, _p) => (types: _t, prototype: _p))
                .Where(_t => _t.types.Length != 1)
                .Select(_t => new GlobalTypeInferenceProblem(_t.types, _t.prototype))
                .ThrowErrors();
            return types.Select(_t => _t.Single()).ToImmutableArray();
        }

        private static VariableDeclarationSyntax Replace(VariableDeclarationSyntax _declaration, ITypeSymbol _type)
        {
            TypeSyntax type = SyntaxFactory.ParseTypeName(_type.ToDisplayString());
            TypeArgumentListSyntax typeArguments = SyntaxFactoryUtils.TypeArguments(type).OwnAs(_declaration.Type);
            {
                QualifiedNameSyntax name = (QualifiedNameSyntax)_declaration.Type;
                _declaration = _declaration.WithType(name.WithRight(SyntaxFactory.GenericName(name.Right.Identifier, typeArguments)));
            }
            {
                InvocationExpressionSyntax initializer = (InvocationExpressionSyntax)_declaration.Variables.Single().Initializer!.Value;
                GenericNameSyntax methodName = (GenericNameSyntax)((MemberAccessExpressionSyntax)initializer.Expression).Name;
                _declaration = _declaration.ReplaceNode(methodName, methodName.WithTypeArgumentList(typeArguments));
            }
            return _declaration.OwnAs(_declaration);
        }

        private static CompilationUnitSyntax ReplaceAll(CompilationUnitSyntax _root, ImmutableArray<VariableDeclarationSyntax> _declarations, IEnumerable<ITypeSymbol> _types)
        {
            ImmutableDictionary<VariableDeclarationSyntax, ITypeSymbol> map = _declarations
                .Zip(_types, (_d, _t) => (declaration: _d, type: _t))
                .ToImmutableDictionary(_e => _e.declaration, _e => _e.type);
            return _root.ReplaceNodes(_declarations, (_old, _new) => Replace(_new, map[_old]));
        }

        internal static CompilationUnitSyntax Rewrite(Session _session)
        {
            ImmutableArray<GlobalDefinition> globals = _session.Globals.Where(_g => _g.Prototype.Global.Type.IsVar).ToImmutableArray();
            if (globals.IsEmpty)
            {
                return _session.Syntax;
            }
            ImmutableArray<GlobalSyntax> syntax = GetSyntax(_session.Class, globals, _session.Tagger);
            ImmutableArray<ITypeSymbol> types = InferSingleType(syntax, globals.Select(_g => _g.Prototype), _session.Semantics!, _session.RuntimeReference);
            return ReplaceAll(_session.Syntax, syntax.Select(_s => _s.Declaration).ToImmutableArray(), types);
        }

    }


}
