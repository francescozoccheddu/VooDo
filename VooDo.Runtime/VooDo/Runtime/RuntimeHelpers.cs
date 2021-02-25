namespace VooDo.Runtime
{
    public static class RuntimeHelpers
    {

        public static Variable<TValue> CreateVariable<TValue>(bool _isConstant, string _name, TValue _value = default!)
            => new(_isConstant, _name, _value!);

        public static TValue SetControllerAndGetValue<TValue>(Variable<TValue> _variable, IControllerFactory<TValue> _controllerFactory)
        {
            _variable.ControllerFactory = _controllerFactory;
            return _variable.Value;
        }

        public const string typedRunMethodName = nameof(TypedProgram<object>.TypedRun);
        public const string runMethodName = nameof(Program.Run);
        public const string hooksPropertyName = nameof(Program.GeneratedHooks);
        public const string variablesPropertyName = nameof(Program.GeneratedVariables);
        public const string subscribeHookMethodName = nameof(Program.SubscribeHook);

    }
}
