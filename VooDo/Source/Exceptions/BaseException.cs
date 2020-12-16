
using System;
using System.Linq;

namespace VooDo.Exceptions
{

    public abstract class BaseException : Exception
    {

        public BaseException(string _message) : base(_message) { }

        public BaseException(string _message, BaseException _inner) : base(_message, _inner) { }


    }

}
