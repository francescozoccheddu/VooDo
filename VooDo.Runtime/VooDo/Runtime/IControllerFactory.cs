namespace VooDo.Runtime
{

    public interface IControllerFactory
    {

        IController CreateController(IVariable _variable);

    }

    public interface IControllerFactory<TValue> : IControllerFactory where TValue : notnull
    {

        IController<TValue> CreateController(Variable<TValue> _variable);

    }

}
