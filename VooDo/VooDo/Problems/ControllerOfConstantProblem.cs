
using VooDo.AST;

namespace VooDo.Problems
{

    public class ControllerOfConstantProblem : Problem
    {

        internal ControllerOfConstantProblem(Node _source)
            : base(EKind.Semantic, ESeverity.Error, "Cannot apply $ operator to a constant", _source) { }

    }



}
