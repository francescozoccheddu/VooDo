namespace VooDo.Runtime
{

    public interface IHook
    {

        void Subscribe(object _object);

        void Unsubscribe();

        IHookListener? Listener { set; }

        IHook Clone();

    }

}
