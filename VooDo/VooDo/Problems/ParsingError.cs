using VooDo.AST;

namespace VooDo.Problems
{

    public class ParsingError : Problem
    {

        internal ParsingError(string _description, CodeOrigin _origin)
            : base(EKind.Syntactic, ESeverity.Error, _description, _origin) { }

    }

}
