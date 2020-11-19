
namespace BS.Exceptions.Runtime
{

    public abstract class StatementFailure : RuntimeFailure
    {

        public StatementFailure(string _message) : base(_message)
        {
        }

        public StatementFailure(string _message, RuntimeFailure _failure) : base(_message, _failure)
        {
        }

        // TODO Statement prop

    }

}
