using Microsoft.CodeAnalysis;

using System.Collections.Generic;

using VooDo.Compilation;
using VooDo.Factory;
using VooDo.Compilation;

namespace VooDo.AST
{

    public abstract record NodeOrIdentifier
    {

        public virtual bool Equals(NodeOrIdentifier? _other) => true;
        public override int GetHashCode() => 0;

        public Origin Origin { get; init; } = default;
        public abstract override string ToString();

        public abstract IEnumerable<NodeOrIdentifier> Children { get; }
        internal abstract SyntaxNodeOrToken EmitNodeOrToken(Scope _scope, Marker _marker);

    }

    public abstract record Node : NodeOrIdentifier
    {

        internal sealed override SyntaxNodeOrToken EmitNodeOrToken(Scope _scope, Marker _marker) => EmitNode(_scope, _marker);
        internal abstract SyntaxNode EmitNode(Scope _scope, Marker _marker);

    }

}
