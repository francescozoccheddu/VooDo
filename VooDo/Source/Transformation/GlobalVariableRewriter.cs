using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Factory;
using VooDo.Factory.Syntax;

namespace VooDo.Transformation
{

    internal static class GlobalVariableRewriter
    {

        private sealed class Rewriter : CSharpSyntaxRewriter
        {

            private readonly SemanticModel m_semantics;
            private readonly INamedTypeSymbol m_globSymbol;
            private readonly IMethodSymbol m_cofSymbol;
            private readonly IMethodSymbol m_gexprSymbol;
            private ComplexTypeOrVar? m_declaringGlobalType;
            private readonly List<Global> m_globals = new List<Global>();

            public Rewriter(SemanticModel _semantics)
            {
                m_semantics = _semantics;
                MetadataReference runtimeReference = m_semantics.Compilation.References.Single(_r => _r.Properties.Aliases.Contains(Identifiers.referenceAlias));
                IAssemblySymbol runtimeSymbol = (IAssemblySymbol) m_semantics.Compilation.GetAssemblyOrModuleSymbol(runtimeReference)!;
                INamedTypeSymbol metaSymbol = runtimeSymbol.GetTypeByMetadataName(QualifiedType.FromType(typeof(Meta)).ToString())!;
                m_globSymbol = metaSymbol.GetTypeMembers(SimpleType.FromType(typeof(Meta.Glob<>), true).ToString()).Single();
                m_cofSymbol = (IMethodSymbol) metaSymbol.GetMembers(nameof(Meta.cof)).Single();
                m_gexprSymbol = (IMethodSymbol) metaSymbol.GetMembers(nameof(Meta.gexpr)).Single();
            }

            private ITypeSymbol? TryGetGlobalType(TypeSyntax _syntax)
                => m_semantics.GetTypeInfo(_syntax).Type is INamedTypeSymbol type
                && m_globSymbol.Equals(type.ConstructedFrom, SymbolEqualityComparer.IncludeNullability)
                ? type.TypeArguments[0]
                : null;

            private bool IsGExprMethod(ExpressionSyntax _syntax)
            {
                return false;
            }

            public override SyntaxNode? VisitDeclarationExpression(DeclarationExpressionSyntax _node)
            {
                if (TryGetGlobalType(_node.Type) is not null)
                {
                    throw new InvalidOperationException("Cannot declare global out arguments"); // TODO Emit diagnostic
                }
                return base.VisitDeclarationExpression(_node);
            }

            public override SyntaxNode? VisitVariableDeclarator(VariableDeclaratorSyntax _node)
            {
                if (m_declaringGlobalType is not null)
                {
                    VariableDeclaratorSyntax newNode = (VariableDeclaratorSyntax) base.VisitVariableDeclarator(_node)!;
                    ExpressionSyntax? initializer = newNode.Initializer?.Value; // TODO Store initializer
                    EqualsValueClauseSyntax newInitializer =
                        SyntaxFactory.EqualsValueClause(SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.ThisExpression(),
                                    SyntaxFactory.GenericName(
                                        SyntaxFactory.Identifier(nameof(Script.SubscribeHook)),
                                        SyntaxFactory.TypeArgumentList(
                                            SyntaxFactory.SingletonSeparatedList(
                                                m_declaringGlobalType.ToTypeSyntax())))),
                                SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.LiteralExpression(
                                                SyntaxKind.NumericLiteralExpression,
                                                SyntaxFactory.Literal(m_globals.Count)))))));
                    m_globals.Add(new Global(m_declaringGlobalType, Identifier.FromSyntax(_node.Identifier)));
                    return newNode.WithInitializer(newInitializer.WithOrigin(Origin.Transformation));
                }
                else
                {
                    return base.VisitVariableDeclarator(_node);
                }
            }

            public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax _node)
            {
                return base.VisitInvocationExpression(_node);
            }

            public override SyntaxNode? VisitVariableDeclaration(VariableDeclarationSyntax _node)
            {
                if (TryGetGlobalType(_node.Type) is ITypeSymbol type)
                {
                    if (m_declaringGlobalType is not null)
                    {
                        throw new InvalidOperationException("Nested global declarations"); // TODO Emit diagnostic
                    }
                    TypeSyntax typeSyntax = _node.Type
                        .DescendantNodesAndSelf()
                        .OfType<GenericNameSyntax>()
                        .First()
                        .TypeArgumentList.Arguments[0];
                    m_declaringGlobalType = ComplexTypeOrVar.FromSyntax(typeSyntax);
                    VariableDeclarationSyntax newNode = (VariableDeclarationSyntax) base.VisitVariableDeclaration(_node)!;
                    m_declaringGlobalType = null;
                    return newNode.WithType(typeSyntax);
                }
                else
                {
                    return base.VisitVariableDeclaration(_node);
                }
            }

        }

        internal static CompilationUnitSyntax Rewrite(SemanticModel _semantics, out ImmutableArray<Global> _globals)
        {
            if (_semantics is null)
            {
                throw new ArgumentNullException(nameof(_semantics));
            }
            Rewriter rewriter = new Rewriter(_semantics);
            SyntaxNode newRoot = rewriter.Visit(_semantics.SyntaxTree.GetRoot());
            _globals = default;
            return (CompilationUnitSyntax) newRoot;
        }

    }

}
