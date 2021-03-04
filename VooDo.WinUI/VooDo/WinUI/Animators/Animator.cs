using System.Collections.Generic;

using VooDo.Runtime;

namespace VooDo.WinUI.Animators
{

    public abstract class Animator<TValue> : Controller<TValue>, IAnimator where TValue : notnull
    {

        public abstract record Factory<TAnimator>() : IControllerFactory<TValue> where TAnimator : Animator<TValue>
        {

            protected abstract TAnimator Create(TValue? _value);

            protected abstract void Set(TAnimator _animator);

            IController<TValue> IControllerFactory<TValue>.CreateController(Variable<TValue> _variable)
            {
                TAnimator animator = (TAnimator)(_variable.Controller is TAnimator old
                               ? ((IControllerFactory<TValue>)old).CreateController(_variable)
                               : Create(_variable.Value ?? default));
                Set(animator);
                animator.RequestUpdate();
                return animator;
            }

            IController IControllerFactory.CreateController(IVariable _variable) => ((IControllerFactory<TValue>)this).CreateController((Variable<TValue>)_variable);

        }

        public Animator(TValue? _value)
        {
            SetValue(_value, false);
        }

        private bool m_running;

        private void UpdateInternal(double _deltaTime)
        {
            SetRunning(Update(_deltaTime));
        }

        protected abstract bool Update(double _deltaTime);

        protected void RequestUpdate()
        {
            if (Variable is not null)
            {
                UpdateInternal(0);
            }
        }

        IProgram IAnimator.Program => Variable!.Program;
        void IAnimator.Update(double _deltaTime) => UpdateInternal(_deltaTime);

        protected sealed override Controller<TValue> CloneForVariable(Variable<TValue> _newVariable)
        {
            Animator<TValue> clone = (Animator<TValue>)MemberwiseClone();
            clone.m_running = false;
            clone.Cloned();
            return clone;
        }

        protected sealed override void PrepareForVariable(Variable<TValue> _variable, Variable<TValue>? _oldVariable) { }

        protected virtual void Cloned() { }

        private void SetRunning(bool _running)
        {
            if (m_running != _running)
            {
                m_running = _running;
                if (_running)
                {
                    AnimatorManager.RegisterAnimator(this);
                }
                else
                {
                    AnimatorManager.UnregisterAnimator(this);
                }
            }
        }

        protected sealed override void Attached(Variable<TValue> _variable)
        {
            RequestUpdate();
        }

        protected sealed override void Detached(Variable<TValue> _variable)
        {
            SetRunning(false);
        }

    }

    public abstract class TargetedAnimator<TValue> : Animator<TValue> where TValue : notnull
    {

        public abstract record TargetedFactory<TAnimator>(TValue Target) : Factory<TAnimator> where TAnimator : TargetedAnimator<TValue>
        {

            protected override void Set(TAnimator _animator) => _animator.Target = Target;

        }

        public TValue Target { get; protected set; }

        protected TargetedAnimator(TValue _value, TValue _target) : base(_value)
        {
            Target = _target;
        }

        protected sealed override bool Update(double _deltaTime)
        {
            TValue oldValue = Value!;
            TValue newValue = Update(_deltaTime, oldValue, Target);
            SetValue(newValue, true);
            return !EqualityComparer<TValue?>.Default.Equals(newValue, Target);
        }

        protected abstract TValue Update(double _deltaTime, TValue _current, TValue _target);

        protected void JumpToTarget(bool _notifyValueChanged = true) => SetValue(Target, _notifyValueChanged);

        public sealed override void Freeze(IVariable _variable)
        {
            JumpToTarget(false);
            Detach();
        }

    }

    public interface IAnimatorWithSpeed<TSpeed> where TSpeed : notnull
    {

        TSpeed Speed { get; }

    }

}
