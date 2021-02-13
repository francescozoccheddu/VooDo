
using VooDo.AST;

namespace VooDo.Errors.Problems
{

    public class SyntaxError : Problem
    {
        internal SyntaxError(NodeOrIdentifier _source, string _description) : base(EKind.Syntactic, ESeverity.Error, _source, _description) { }
    }

    public class ChildSyntaxError : SyntaxError
    {

        public NodeOrIdentifier Child { get; }

        internal ChildSyntaxError(NodeOrIdentifier _source, NodeOrIdentifier _child, string _description) : base(_source, _description)
        {
            Child = _child;
        }
    }

}
