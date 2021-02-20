
using VooDo.Runtime;

namespace VooDo.WinUI.Interfaces
{

    public delegate void TargetDiscontinuedEventHandler(ITarget _target);

    public interface ITarget
    {

        void AttachProgram(Program _program);
        void DetachProgram();

        event TargetDiscontinuedEventHandler? OnTargetDiscontinued;

    }

}
