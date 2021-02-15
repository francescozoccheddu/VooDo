
using VooDo.AST;

namespace VooDo.Problems
{

    public class ControllerOfNonGlobalProblem : SourceProblem
    {

        internal ControllerOfNonGlobalProblem(Node _source)
            : base(EKind.Semantic, ESeverity.Error, "Cannot apply $ operator to non-global variable", _source) { }

    }



}
