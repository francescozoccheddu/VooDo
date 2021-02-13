namespace VooDo.Errors.Problems
{

    public class NoSemanticsProblem : Problem
    {

        internal NoSemanticsProblem() : base(EKind.Semantic, ESeverity.Error, null, "Unable to retrieve critical semantic information") { }

    }

}
