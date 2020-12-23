namespace VooDo.Source.Runtime
{
    public interface IControllerFactory
    {

        IController Create(IController _oldController, object _value);

    }
}
