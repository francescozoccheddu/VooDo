
using VooDo.AST;
using VooDo.AST.Names;

namespace VooDo.Problems
{

    public class VariableRedefinitionProblem : Problem
    {

        public Identifier Name { get; }

        internal VariableRedefinitionProblem(Node _source, Identifier _name)
            : base(EKind.Semantic, ESeverity.Error, $"Name '{_name}' already exists", _source)
        {
            Name = _name;
        }

    }


}
