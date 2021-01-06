using VooDo.AST;

namespace VooDo.Runtime.Hooks
{
    public interface IHookProvider
    {

        IHook Subscribe(object _instance, Name _property);

    }
}
