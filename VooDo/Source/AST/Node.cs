using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Linq;

using VooDo.Compilation.Emission;
using VooDo.Errors.Problems;

namespace VooDo.AST
{

    public abstract record NodeOrIdentifier
    {

        public virtual bool Equals(NodeOrIdentifier? _other) => true;
        public override int GetHashCode() => 0;

        internal NodeOrIdentifier() { }

        public Origin Origin { get; init; } = Origin.Unknown;

        internal abstract SyntaxNodeOrToken EmitNodeOrToken(Scope _scope, Marker _marker);

        public virtual IEnumerable<NodeOrIdentifier> Children
            => Enumerable.Empty<NodeOrIdentifier>();

        public IEnumerable<Problem> GetSyntaxProblems()
            => GetSelfSyntaxProblems().Concat(Children.SelectMany(_c => _c.GetSyntaxProblems()));

        protected virtual IEnumerable<Problem> GetSelfSyntaxProblems()
            => Enumerable.Empty<Problem>();

        public abstract NodeOrIdentifier ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map);

    }

    public abstract record Node : NodeOrIdentifier
    {

        public abstract override Node ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map);

        internal sealed override SyntaxNodeOrToken EmitNodeOrToken(Scope _scope, Marker _marker) => EmitNode(_scope, _marker);
        internal abstract SyntaxNode EmitNode(Scope _scope, Marker _marker);

    }

}
