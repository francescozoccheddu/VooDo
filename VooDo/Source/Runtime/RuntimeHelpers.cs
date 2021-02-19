namespace VooDo.Runtime
{
    public static class RuntimeHelpers
    {

        public static Variable<TValue> CreateVariable<TValue>(bool _isConstant, string _name, TValue _value = default)
            => new Variable<TValue>(_isConstant, _name, _value!);

        public static TValue SetControllerAndGetValue<TValue>(Variable<TValue> _variable, IControllerFactory<TValue> _controllerFactory)
        {
            _variable.ControllerFactory = _controllerFactory;
            return _variable.Value;
        }

    }
}
