
using BS.AST.Expressions;

namespace BS.Exceptions.Runtime.Expressions
{

    public sealed class UnassignableError : AssignmentFailure
    {

        public UnassignableError(Expr _target) : base("Invalid assignment target", _target) { }

    }

}
