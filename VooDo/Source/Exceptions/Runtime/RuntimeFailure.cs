
namespace VooDo.Exceptions.Runtime
{

    public abstract class RuntimeFailure : BaseException
    {

        public RuntimeFailure(string _message) : base(_message)
        {
        }

        public RuntimeFailure(string _message, RuntimeFailure _failure) : base(_message, _failure)
        {
        }

        // TODO Statement prop

    }

}
