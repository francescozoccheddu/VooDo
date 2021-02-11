
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;

using VooDo.Compilation;
using VooDo.Language.AST.Names;
using VooDo.Language.Linking;
using VooDo.Utils;

namespace VooDo.Language.AST.Directives
{

    public sealed record UsingStaticDirective(QualifiedType Type) : UsingDirective
    {

        #region Members

        private QualifiedType m_type = Type.Assert(_t => !_t.IsArray && !_t.IsNullable, "Nullable or array type");
        public QualifiedType Type
        {
            get => m_type;
            init => m_type = value.Assert(_t => !_t.IsArray && !_t.IsNullable, "Nullable or array type");
        }

        #endregion

        #region Overrides

        internal override UsingDirectiveSyntax EmitNode(Scope _scope, Marker _marker)
            => SyntaxFactory.UsingDirective(
                    SyntaxFactory.Token(SyntaxKind.StaticKeyword),
                    null,
                    (NameSyntax) Type.EmitNode(_scope, _marker))
                .Own(_marker, this);
        public override IEnumerable<QualifiedType> Children => new QualifiedType[] { Type };
        public override string ToString() => $"{GrammarConstants.usingKeyword} {GrammarConstants.staticKeyword} {Type}{GrammarConstants.statementEndToken}";

        #endregion

    }

}
