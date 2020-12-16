
namespace VooDo.Exceptions.Runtime
{

    public abstract class EvaluationFailure : RuntimeFailure
    {

        public EvaluationFailure(string _message) : base(_message)
        {
        }

        public EvaluationFailure(string _message, RuntimeFailure _innerException) : base(_message, _innerException)
        {
        }

        // TODO Statement prop

    }

}
