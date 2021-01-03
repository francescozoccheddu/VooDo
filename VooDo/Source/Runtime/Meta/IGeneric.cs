using System;

namespace VooDo.Runtime.Meta
{
    public interface IGeneric
    {

        object Specialize(Type[] _arguments);

    }

}
