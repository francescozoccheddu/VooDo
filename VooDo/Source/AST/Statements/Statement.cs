using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Collections.Immutable;

using VooDo.Compiling.Emission;

namespace VooDo.AST.Statements
{

    public abstract record Statement : BodyNode
    {

        internal abstract override StatementSyntax EmitNode(Scope _scope, Tagger _tagger);
        internal abstract IEnumerable<StatementSyntax> EmitNodes(Scope _scope, Tagger _tagger);

    }

    public abstract record SingleStatement : Statement
    {

        internal abstract override StatementSyntax EmitNode(Scope _scope, Tagger _tagger);

        internal sealed override IEnumerable<StatementSyntax> EmitNodes(Scope _scope, Tagger _tagger)
            => ImmutableArray.Create(EmitNode(_scope, _tagger));

    }

    public abstract record MultipleStatements : Statement
    {

        internal sealed override StatementSyntax EmitNode(Scope _scope, Tagger _tagger)
            => SyntaxFactory.Block(EmitNodes(_scope, _tagger)).Own(_tagger, this);

        internal abstract override IEnumerable<StatementSyntax> EmitNodes(Scope _scope, Tagger _tagger);

    }

}
