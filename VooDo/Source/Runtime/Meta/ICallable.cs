using System;

namespace VooDo.Runtime.Meta
{
    public interface ICallable
    {

        object Call(object[] _arguments, Type[] _types = null);

    }

}
