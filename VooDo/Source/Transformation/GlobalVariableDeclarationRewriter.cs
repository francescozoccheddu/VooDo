using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Factory;
using VooDo.Factory.Syntax;

namespace VooDo.Transformation
{

    internal static class GlobalVariableDeclarationRewriter
    {

        private sealed class Rewriter : CSharpSyntaxRewriter
        {

            private readonly SemanticModel m_semantics;
            private readonly INamedTypeSymbol m_globSymbol;
            private readonly IMethodSymbol m_cofSymbol;
            private readonly IMethodSymbol m_gexprSymbol;

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

            public override SyntaxNode VisitVariableDeclaration(VariableDeclarationSyntax _node)
            {
                if (m_semantics.GetTypeInfo(_node.Type).Type is INamedTypeSymbol type && m_globSymbol.Equals(type.ConstructedFrom, SymbolEqualityComparer.Default))
                {

                }
                return base.VisitVariableDeclaration(_node);
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
