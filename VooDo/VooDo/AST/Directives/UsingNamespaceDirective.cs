
using System;
using System.Collections.Generic;

using VooDo.AST.Names;
using VooDo.AST.Statements;
using VooDo.Utils;

namespace VooDo.AST.Directives
{

    public sealed record UsingNamespaceDirective(Identifier? Alias, Namespace Namespace) : UsingDirective
    {

        
        public UsingNamespaceDirective(Namespace _namespace) : this(null, _namespace) { }

        
        
        public bool HasAlias => Alias is not null;

        
        
        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
        {
            Identifier? newAlias = (Identifier?) _map(Alias);
            Namespace newNamespace = (Namespace) _map(Namespace).NonNull();
            if (ReferenceEquals(newAlias, Alias) && ReferenceEquals(newNamespace, Namespace))
            {
                return this;
            }
            else
            {
                return this with
                {
                    Alias = newAlias,
                    Namespace = newNamespace
                };
            }
        }


        public override IEnumerable<Node> Children => HasAlias ? new Node[] { Alias!, Namespace } : new Node[] { Namespace };

        public override string ToString() => $"{GrammarConstants.usingKeyword} "
            + (HasAlias ? $"{Alias} {AssignmentStatement.EKind.Simple.Token()} " : "")
            + Namespace
            + GrammarConstants.statementEndToken;

        
    }

}
