using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using static VooDo.Errors.Problems.Problem;

namespace VooDo.Errors.Problems
{

    public static class ProblemExtensions
    {

        internal static ProblemsException AsThrowable(this IEnumerable<Problem> _problems)
            => new ProblemsException(_problems.ToImmutableArray());

        internal static ProblemsException AsThrowable(this Problem _problem)
            => new ProblemsException(_problem);

        public static void ThrowErrors(this IEnumerable<Problem> _problems)
        {
            ImmutableArray<Problem> errors = _problems.Errors().ToImmutableArray();
            if (!errors.IsEmpty)
            {
                throw errors.AsThrowable();
            }
        }

        public static IEnumerable<Problem> Syntactic(this IEnumerable<Problem> _problems)
            => _problems.OfKind(EKind.Syntactic);

        public static IEnumerable<Problem> Semantic(this IEnumerable<Problem> _problems)
            => _problems.OfKind(EKind.Semantic);

        public static IEnumerable<Problem> OfKind(this IEnumerable<Problem> _problems, EKind _kind)
            => _problems.Where(_p => _p.Kind == _kind);

        public static IEnumerable<Problem> Errors(this IEnumerable<Problem> _problems)
            => _problems.OfSeverity(ESeverity.Error);

        public static IEnumerable<Problem> Warnings(this IEnumerable<Problem> _problems)
            => _problems.OfSeverity(ESeverity.Warning);

        public static IEnumerable<Problem> OfSeverity(this IEnumerable<Problem> _problems, ESeverity _severity)
            => _problems.Where(_p => _p.Severity == _severity);

    }

}
