using System;

namespace VooDo.Errors
{

    public abstract class UserCodeException : VooDoException
    {

        internal UserCodeException(string _message, Exception _exception) : base(_message, _exception) { }

    }

}
