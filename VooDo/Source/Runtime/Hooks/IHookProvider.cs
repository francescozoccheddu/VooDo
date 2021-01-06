using VooDo.AST;

namespace VooDo.Runtime.Hooks
{
    public interface IHookProvider
    {

        IHook Subscribe(Eval _instance, Name _property);

    }
}
