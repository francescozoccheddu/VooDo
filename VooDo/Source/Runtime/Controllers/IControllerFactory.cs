namespace VooDo.Runtime.Controllers
{
    public interface IControllerFactory
    {

        IController Create(IController _oldController, Eval _value);

    }
}
