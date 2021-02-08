
using System.Collections.Generic;

using VooDo.Language.AST.Names;
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

        public override IEnumerable<Node> Children => new Node[] { Type };
        public override string ToString() => $"{GrammarConstants.usingKeyword} {GrammarConstants.staticKeyword} {Type}{GrammarConstants.statementEndToken}";

        #endregion

    }

}
