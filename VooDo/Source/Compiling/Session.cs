using System.Collections.Generic;
using System.Collections.Immutable;

using VooDo.Compiling.Emission;
using VooDo.Problems;

namespace VooDo.Compiling
{

    internal sealed class Session
    {

        private readonly List<Problem> m_problems = new List<Problem>();

        internal Compilation Compilation { get; }


        internal Tagger Tagger { get; } = new Tagger();

        internal void AddProblem(Problem _problem)
            => m_problems.Add(_problem);

        internal void AddProblem(IEnumerable<Problem> _problems)
            => m_problems.AddRange(_problems);

        internal ImmutableArray<Problem> GetProblems()
            => m_problems.ToImmutableArray();

        internal Session(Compilation _compilation)
        {
            Compilation = _compilation;
        }

    }

}
