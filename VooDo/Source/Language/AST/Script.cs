using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Language.AST.Directives;
using VooDo.Language.AST.Statements;
using VooDo.Language.Linking;
using VooDo.Utils;

namespace VooDo.Language.AST
{

    public sealed record Script(ImmutableArray<UsingDirective> Usings, BlockStatement Body) : Node
    {

        #region Members

        private ImmutableArray<UsingDirective> m_usings = Usings.EmptyIfDefault();
        public ImmutableArray<UsingDirective> Usings
        {
            get => m_usings;
            init => m_usings = value.EmptyIfDefault();
        }

        internal override CompilationUnitSyntax Emit(Scope _scope, ImmutableArray<SyntaxNode> _children, EventContext? _eventContext, HookContext? _hookContext)
        {

        }

        #endregion

        #region Overrides

        public override IEnumerable<Node> Children => ((IEnumerable<Node>) Usings).Append(Body);
        public override string ToString() => Usings.Aggregate("", (_a, _u) => $"{_a}{_u}\n") + Body;

        #endregion

    }

}
