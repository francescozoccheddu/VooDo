using System;

namespace VooDo.Runtime
{

    public interface IController : IControllerFactory
    {

        void OnAttach(IVariable _variable);

        void OnDetach(IVariable _variable);

        object? Value { get; }

        void Freeze(IVariable _program);

    }

    public interface IController<TValue> : IController, IControllerFactory<TValue> where TValue : notnull
    {

        void OnAttach(Variable<TValue> _variable);

        void OnDetach(Variable<TValue> _variable);

        new TValue? Value { get; }

    }

    public abstract class Controller<TValue> : IController<TValue> where TValue : notnull
    {

        protected Variable<TValue>? Variable { get; private set; }
        public TValue? Value { get; private set; }

        object? IController.Value => Value;
        IController IControllerFactory.CreateController(IVariable _variable) => ((IController<TValue>)this).CreateController(_variable.OfType<TValue>());
        void IController.OnAttach(IVariable _variable) => ((IController<TValue>)this).OnAttach(_variable.OfType<TValue>());
        void IController.OnDetach(IVariable _variable) => ((IController<TValue>)this).OnDetach(_variable.OfType<TValue>());

        TValue? IController<TValue>.Value => Value;

        IController<TValue> IControllerFactory<TValue>.CreateController(Variable<TValue> _variable)
        {
            bool reuse = _variable == Variable;
            Controller<TValue> clone = reuse ? this : CloneForVariable(_variable);
            if (!reuse)
            {
                clone.Variable = null;
                clone.PrepareForVariable(_variable, Variable);
            }
            return clone;
        }

        void IController<TValue>.OnAttach(Variable<TValue> _variable)
        {
            if (Variable is not null)
            {
                throw new InvalidOperationException("Controller is already attached to a variable");
            }
            Variable = _variable;
            Attached(_variable);
        }

        void IController<TValue>.OnDetach(Variable<TValue> _variable)
        {
            if (Variable != _variable)
            {
                throw new InvalidOperationException("Controller is not attached to this variable");
            }
            Detached(_variable);
            Variable = null;
        }

        protected virtual Controller<TValue> CloneForVariable(Variable<TValue> _newVariable) => (Controller<TValue>)MemberwiseClone();
        protected virtual void Detached(Variable<TValue> _variable) { }
        protected virtual void Attached(Variable<TValue> _variable) { }
        protected virtual void PrepareForVariable(Variable<TValue> _variable, Variable<TValue>? _oldVariable) { }

        protected void SetValue(TValue? _value, bool _notifyValueChanged = true)
        {
            Value = _value;
            if (_notifyValueChanged)
            {
                Variable?.NotifyChanged();
            }
        }

        protected void Detach()
        {
            if (Variable is not null)
            {
                Variable.Value = Value;
            }
        }

        public virtual void Freeze(IVariable _variable)
        {
            if (Variable != _variable)
            {
                throw new InvalidOperationException("Controller is not attached to this variable");
            }
            Detach();
        }

    }

}
