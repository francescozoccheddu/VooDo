using BS.AST.Expressions;


namespace BS.Exceptions.Runtime.Expressions
{

    public abstract class AssignmentFailure : RuntimeFailure
    {

        public AssignmentFailure(string _message, Expr _target) : base(_message) { }
        public AssignmentFailure(string _message, Expr _target, EvaluationFailure _failure) : base(_message, _failure) { }

    }

}
