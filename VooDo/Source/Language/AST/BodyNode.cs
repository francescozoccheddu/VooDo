using Microsoft.CodeAnalysis;

using System.Collections.Generic;

using VooDo.Language.Linking;

namespace VooDo.Language.AST
{

    public abstract record BodyNodeOrIdentifier : Node
    {

        public abstract override IEnumerable<BodyNodeOrIdentifier> Children { get; }
        internal abstract SyntaxNodeOrToken EmitNodeOrToken(Scope _scope, Marker _marker);

    }

    public abstract record BodyNode : BodyNodeOrIdentifier
    {

        internal sealed override SyntaxNodeOrToken EmitNodeOrToken(Scope _scope, Marker _marker) => EmitNode(_scope, _marker);
        internal abstract SyntaxNode EmitNode(Scope _scope, Marker _marker);

    }

}
