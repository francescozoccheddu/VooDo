
using VooDo.AST;

namespace VooDo.Problems
{

    public class SyntaxError : Problem
    {
        internal SyntaxError(Node _source, string _description) : base(EKind.Syntactic, ESeverity.Error, _description, _source) { }
    }

    public class ChildSyntaxError : SyntaxError
    {

        public Node Child { get; }

        internal ChildSyntaxError(Node _source, Node _child, string _description) : base(_source, _description)
        {
            Child = _child;
        }
    }

}
