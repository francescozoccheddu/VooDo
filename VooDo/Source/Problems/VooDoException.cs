using System;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Utils;

namespace VooDo.Problems
{

    public sealed class VooDoException : Exception
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
            else
            {
                string[] groups = errors
                    .GroupBy(_p => _p.Kind)
                    .OrderBy(_g => _g.Key)
                    .Select(_g => (kind: _g.Key, count: _g.Count()))
                    .Select(_g => $"{_g.count} {_g.kind} error{(_g.count > 1 ? "s" : "")}")
                    .ToArray();
                return groups.Length > 1
                    ? $"{string.Join(", ", groups)} and {groups[^1]}"
                    : groups[0];
            }
        }

        public ImmutableArray<Problem> Problems { get; }

        public override string Message => GetMessage(Problems);

        internal VooDoException(Problem _problem, Exception? _innerException = null) : this(ImmutableArray.Create(_problem), _innerException) { }

        internal VooDoException(ImmutableArray<Problem> _problems, Exception? _innerException = null) : base(GetMessage(_problems), _innerException)
        {
            Problems = _problems;
        }

    }

}
