namespace VooDo.Runtime
{
    public interface IController
    {

        object Value { get; set; }

        void RegisterBinding(Env.Binding _binding);

        void UnregisterBinding(Env.Binding _binding);

    }
}
