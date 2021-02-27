
using VooDo.Runtime;

namespace VooDo.WinUI.Animators
{

    internal interface IAnimator
    {

        void Update(double _deltaTime);

        IProgram Program { get; }

    }

}
