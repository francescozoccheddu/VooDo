using System;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Utils;

namespace VooDo.Problems
{

    public class VooDoException : Exception
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

        internal VooDoException(Problem _problem) : this(ImmutableArray.Create(_problem)) { }

        internal VooDoException(ImmutableArray<Problem> _problems) : base(GetMessage(_problems))
        {
            Problems = _problems;
        }

    }

}
