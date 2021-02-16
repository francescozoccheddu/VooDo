
using System;

using VooDo.AST;
using VooDo.Runtime;

namespace VooDo.WinUI.Interfaces
{

    public delegate void TargetDiscontinuedEventHandler(Target _target);

    public abstract class Target
    {

        internal Target() { }

        protected internal virtual void AttachProgram(Program _program) { }
        protected internal virtual void DetachProgram() { }
        protected internal virtual Script ProcessScript(Script _script) => _script;
        public virtual Type ReturnType => typeof(void);

        public event TargetDiscontinuedEventHandler? OnTargetDiscontinued;

        protected void NotifyDiscontinued()
            => OnTargetDiscontinued?.Invoke(this);

    }

}
