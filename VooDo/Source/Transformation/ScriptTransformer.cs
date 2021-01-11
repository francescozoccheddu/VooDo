using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Linq;

using VooDo.Hooks;
using VooDo.Utils;

namespace VooDo.Transformation
{

    public static class ScriptTransformer
    {

        private sealed class RunMethodRewriter : CSharpSyntaxRewriter
        {

            private readonly SemanticModel m_semantics;
            private readonly IMethodSymbol m_runMethodSymbol;
            internal ExpressionHookGluer ExpressionHookGluer { get; }

            internal RunMethodRewriter(SemanticModel _semantics, IHookInitializerProvider _hookInitializerProvider)
            {
                m_semantics = _semantics;
                INamedTypeSymbol scriptSymbol = m_semantics.Compilation.GetTypeByMetadataName(typeof(Script).FullName);
                m_runMethodSymbol = scriptSymbol.GetMembers().OfType<IMethodSymbol>().Where(_m => _m.Name == nameof(Script.Run)).First();
                ExpressionHookGluer = new ExpressionHookGluer(_hookInitializerProvider);
            }

            private bool ShouldGlue(MethodDeclarationSyntax _node)
            {
                IMethodSymbol symbol = m_semantics.GetDeclaredSymbol(_node);
                IMethodSymbol baseSymbol = symbol.OverriddenMethod;
                while (baseSymbol != null)
                {
                    if (baseSymbol.Equals(m_runMethodSymbol, SymbolEqualityComparer.Default))
                    {
                        return true;
                    }
                    baseSymbol = baseSymbol.OverriddenMethod;
                }
                return false;
            }

            public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax _node)
            {
                if (ShouldGlue(_node))
                {
                    BlockSyntax body = _node.Body ?? SyntaxFactory.Block(SyntaxFactory.SingletonList(SyntaxFactory.ExpressionStatement(_node.ExpressionBody.Expression)));
                    BlockSyntax gluedBody = (BlockSyntax) ExpressionHookGluer.Glue(m_semantics, body);
                    return _node.WithBody(gluedBody);
                }
                else
                {
                    return _node;
                }
            }

        }

        private sealed class ClassRewriter : CSharpSyntaxRewriter
        {

            private readonly SemanticModel m_semantics;
            private readonly INamedTypeSymbol m_scriptSymbol;
            private readonly ITypeSymbol m_hookEnumerableSymbol;
            private readonly IHookInitializerProvider m_hookInitializerProvider;

            internal ClassRewriter(SemanticModel _semantics, IHookInitializerProvider _hookInitializerProvider)
            {
                m_semantics = _semantics;
                m_scriptSymbol = m_semantics.Compilation.GetTypeByMetadataName(typeof(Script).FullName);
                m_hookEnumerableSymbol = m_scriptSymbol.Constructors[0].Parameters[0].Type;
                m_hookInitializerProvider = _hookInitializerProvider;
            }

            private bool InheritsScript(INamedTypeSymbol _symbol)
            {
                INamedTypeSymbol baseSymbol = _symbol.BaseType;
                while (baseSymbol != null)
                {
                    if (baseSymbol.Equals(m_scriptSymbol, SymbolEqualityComparer.Default))
                    {
                        return true;
                    }
                    baseSymbol = baseSymbol.BaseType;
                }
                return false;
            }

            private bool IsExpectedBaseConstructor(IMethodSymbol _symbol)
                => _symbol.Parameters.Length == 1 && _symbol.Parameters[0].Type.Equals(m_hookEnumerableSymbol, SymbolEqualityComparer.Default);

            private bool HasExpectedBaseConstructor(INamedTypeSymbol _symbol)
                => _symbol.BaseType?.Constructors.Any(IsExpectedBaseConstructor) ?? false;

            private ConstructorDeclarationSyntax GetDefaultConstructor(INamedTypeSymbol _node)
            {
                IMethodSymbol symbol = _node.Constructors.OfType<IMethodSymbol>().FirstOrDefault(_m => _m.Parameters.Length == 0);
                return (ConstructorDeclarationSyntax) symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
            }

            private static ConstructorInitializerSyntax CreateConstructorInitializer(IEnumerable<IHookInitializer> _hookInitializers)
            {
                ArgumentSyntax argument = SyntaxFactory.Argument(
                    SyntaxFactory.ArrayCreationExpression(
                        SyntaxFactory.ArrayType(SyntaxFactoryHelper.Type(typeof(IHook)))
                        .WithRankSpecifiers(
                            SyntaxFactory.SingletonList(
                                SyntaxFactory.ArrayRankSpecifier(
                                    SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                        SyntaxFactory.OmittedArraySizeExpression())))))
                        .WithInitializer(
                            SyntaxFactory.InitializerExpression(
                                SyntaxKind.ArrayInitializerExpression,
                                SyntaxFactory.SeparatedList(
                                    _hookInitializers.Select(_i => _i.GetHookInitializerSyntax())))));
                return SyntaxFactory.ConstructorInitializer(
                            SyntaxKind.BaseConstructorInitializer,
                            SyntaxFactory.ArgumentList(
                                SyntaxFactory.SingletonSeparatedList(argument)));
            }

            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax _node)
            {
                INamedTypeSymbol symbol = m_semantics.GetDeclaredSymbol(_node);
                if (!InheritsScript(symbol) || !HasExpectedBaseConstructor(symbol))
                {
                    return _node;
                }
                else
                {
                    RunMethodRewriter runMethodRewriter = new RunMethodRewriter(m_semantics, m_hookInitializerProvider);
                    ClassDeclarationSyntax gluedNode = (ClassDeclarationSyntax) runMethodRewriter.Visit(_node);
                    ConstructorDeclarationSyntax constructor = GetDefaultConstructor(symbol);
                    ConstructorInitializerSyntax initializer = CreateConstructorInitializer(runMethodRewriter.ExpressionHookGluer.HookInitializers);
                    if (constructor == null)
                    {
                        constructor = SyntaxFactory.ConstructorDeclaration(_node.Identifier)
                            .WithModifiers(
                                SyntaxFactory.TokenList(
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                            .WithInitializer(initializer)
                            .WithBody(SyntaxFactory.Block());
                        gluedNode = gluedNode.AddMembers(constructor);
                    }
                    else
                    {
                        gluedNode = gluedNode.ReplaceNode(constructor, constructor.WithInitializer(initializer));
                    }
                    return gluedNode;
                }
            }

        }

        private const string c_tempAssemblyName = "ScriptTransformer_VooDo_internal_";

        public struct Options
        {

            public static Options Default { get; }
                = new Options
                {
                    HookInitializerProvider = new HookInitializerList()
                };

            public IHookInitializerProvider HookInitializerProvider { get; set; }

            internal void EnsureValid()
            {
                if (HookInitializerProvider == null)
                {
                    throw new NullReferenceException($"{nameof(HookInitializerProvider)} cannot be null");
                }
            }

        }

        public static SyntaxNode Transform(SemanticModel _semantics, SyntaxNode _root, Options _options)
        {
            if (_semantics == null)
            {
                throw new ArgumentNullException(nameof(_semantics));
            }
            if (_root == null)
            {
                throw new ArgumentNullException(nameof(_root));
            }
            try
            {
                _options.EnsureValid();
            }
            catch (Exception exception)
            {
                throw new ArgumentException(nameof(_options), exception);
            }
            ClassRewriter classRewriter = new ClassRewriter(_semantics, _options.HookInitializerProvider);
            return classRewriter.Visit(_root);
        }

        public static CompilationUnitSyntax Transform(SemanticModel _semantics, Options _options)
        {
            if (_semantics == null)
            {
                throw new ArgumentNullException(nameof(_semantics));
            }
            return (CompilationUnitSyntax) Transform(_semantics, _semantics.SyntaxTree.GetRoot(), _options);
        }

        public static CompilationUnitSyntax Transform(CompilationUnitSyntax _root, Options _options)
        {
            if (_root == null)
            {
                throw new ArgumentNullException(nameof(_root));
            }
            SemanticModel semantics = Compiler.GetSemantics(_root, Compiler.Options.Default);
            return Transform(semantics, _options);
        }

    }

}
