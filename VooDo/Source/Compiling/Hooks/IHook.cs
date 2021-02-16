namespace VooDo.Hooks
{

    public interface IHook
    {

        void Subscribe(object _object);

        void Unsubscribe();

        IHookListener Listener { set; }

    }

}
