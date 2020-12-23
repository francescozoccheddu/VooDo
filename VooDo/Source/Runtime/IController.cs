using VooDo.Runtime;

namespace VooDo.Source.Runtime
{
    public interface IController
    {

        object Value { get; set; }

        void RegisterBinding(Env.Binding _binding);

        void UnregisterBinding(Env.Binding _binding);

    }
}
