
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;

using VooDo.AST.Names;
using VooDo.Compilation;
using VooDo.Compilation.Emission;
using VooDo.Errors.Problems;
using VooDo.Utils;

namespace VooDo.AST.Directives
{

    public sealed record UsingStaticDirective : UsingDirective
    {

        #region Members

        public UsingStaticDirective(QualifiedType _type)
        {
            m_type = Type = _type;
        }

        private QualifiedType m_type;
        public QualifiedType Type
        {
            get => m_type;
            init
            {
                if (value.IsArray || value.IsNullable)
                {
                    throw new ChildSyntaxError(this, value, "Using static directive type cannot be nullable or array").AsThrowable();
                }
                m_type = value;
            }
        }

        #endregion

        #region Overrides

        public override UsingStaticDirective ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map)
        {
            QualifiedType newType = (QualifiedType) _map(Type).NonNull();
            if (ReferenceEquals(newType, Type))
            {
                return this;
            }
            else
            {
                return this with
                {
                    Type = newType
                };
            }
        }

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
