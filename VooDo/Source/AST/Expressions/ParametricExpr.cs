using System.Collections.Generic;
using System.Linq;

using VooDo.Runtime;
using VooDo.Source.Utils;
using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public abstract class ParametricExpr : Expr
    {

        protected ParametricExpr(Expr _source, IEnumerable<Expr> _arguments)
        {
            Ensure.NonNull(_source, nameof(_source));
            Ensure.NonNull(_arguments, nameof(_arguments));
            Arguments = _arguments.ToList().AsReadOnly();
            Ensure.NonNullItems(Arguments, nameof(_arguments));
            Source = _source;
        }

        public Expr Source { get; }
        public IReadOnlyList<Expr> Arguments { get; }

        public override void Unsubscribe(HookManager _hookManager)
        {
            Source.Unsubscribe(_hookManager);
            foreach (Expr arg in Arguments)
            {
                arg.Unsubscribe(_hookManager);
            }
        }

        internal override HashSet<Name> GetVariables()
            => Tree.GetVariables(Source).Union(Tree.GetVariables(Arguments)).ToHashSet();

        public override bool Equals(object _obj)
            => _obj is ParametricExpr expr && Source.Equals(expr.Source) && Arguments.SequenceEqual(expr.Arguments);

        public override int GetHashCode()
            => Identity.CombineHash(Source, Identity.CombineHashes(Arguments));

    }

}
