namespace VooDo.Problems
{

    public class NoSemanticsProblem : Problem
    {

        internal NoSemanticsProblem() : base(EKind.Semantic, ESeverity.Error, "Unable to retrieve critical semantic information") { }

    }

}
