
using System;
using System.Collections.Generic;

using VooDo.AST.Names;
using VooDo.Problems;
using VooDo.Utils;

namespace VooDo.AST.Directives
{

    public sealed record UsingStaticDirective : UsingDirective
    {

        
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

        
        
        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
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

        public override IEnumerable<Node> Children => new QualifiedType[] { Type };
        public override string ToString() => $"{GrammarConstants.usingKeyword} {GrammarConstants.staticKeyword} {Type}{GrammarConstants.statementEndToken}";

        
    }

}
