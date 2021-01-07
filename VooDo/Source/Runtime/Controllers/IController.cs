namespace VooDo.Runtime.Controllers
{
    public interface IController : IControllerFactory
    {

        Eval Value { get; set; }

        void RegisterBinding(Env.Binding _binding);

        void UnregisterBinding(Env.Binding _binding);

    }
}
