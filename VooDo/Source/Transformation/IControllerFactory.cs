namespace VooDo.Transformation
{

    public interface IControllerFactory<TValue>
    {

        Controller<TValue> Create(Variable<TValue> _variable);

    }

}
