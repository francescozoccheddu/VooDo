using System.Collections.Immutable;
using System.Linq;

using VooDo.Errors.Problems;

namespace VooDo.Errors
{

    public class ProblemsException : VooDoException
    {

        private static string GetMessage(ImmutableArray<Problem> _problems)
        {
            ImmutableArray<Problem> errors = _problems.Errors().ToImmutableArray();
            if (errors.IsEmpty)
            {
                return "Unknown error";
            }
            if (errors.Length == 1)
            {
                return _problems[0].GetDisplayMessage();
            }
            int syntaxErrors = _problems.Syntactic().Count();
            int semanticErrors = _problems.Semantic().Count();
            return ((syntaxErrors > 0 ? $"{syntaxErrors} syntax error{(syntaxErrors > 1 ? "s" : "")}" : "")
                + (syntaxErrors > 0 && semanticErrors > 0 ? " and " : "")
                + (semanticErrors > 0 ? $"{semanticErrors} semantic error{(semanticErrors > 1 ? "s" : "")}" : "")).Trim();
        }

        public ImmutableArray<Problem> Problems { get; }

        internal ProblemsException(Problem _problem) : this(ImmutableArray.Create(_problem)) { }

        internal ProblemsException(ImmutableArray<Problem> _problems) : base(GetMessage(_problems))
        {
            Problems = _problems;
        }


    }

}
