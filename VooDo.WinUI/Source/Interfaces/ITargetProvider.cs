using VooDo.WinUI.Xaml;

namespace VooDo.WinUI.Interfaces
{

    public interface ITargetProvider<out TTarget> where TTarget : ITarget
    {

        TTarget? GetTarget(XamlInfo _xamlInfo);

    }

}
