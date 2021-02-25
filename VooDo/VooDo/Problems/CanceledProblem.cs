namespace VooDo.Problems
{

    public class CanceledProblem : Problem
    {

        internal CanceledProblem() : base(EKind.Other, ESeverity.Error, "Operation was canceled") { }

    }

}
