namespace VooDo.Runtime.Hooks
{

    public delegate void HookEventHandler();

    public interface IHook
    {

        event HookEventHandler OnChange;

        void Unsubscribe();

    }

}
