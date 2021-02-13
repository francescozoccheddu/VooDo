using System;

namespace VooDo.Errors
{

    public abstract class VooDoException : Exception
    {

        protected VooDoException(string? _message) : base(_message)
        {
        }

        protected VooDoException(string? _message, Exception? _innerException) : base(_message, _innerException)
        {
        }

    }

}
