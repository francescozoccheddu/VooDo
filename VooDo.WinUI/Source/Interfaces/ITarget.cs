
using VooDo.Runtime;

namespace VooDo.WinUI.Interfaces
{

    public delegate void TargetDiscontinuedEventHandler(ITarget _target);

    public interface ITarget
    {

        internal void AttachProgram(Program _program) { }
        internal void DetachProgram() { }

        internal event TargetDiscontinuedEventHandler? OnTargetDiscontinued;

    }

}
