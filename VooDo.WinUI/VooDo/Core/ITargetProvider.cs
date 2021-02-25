using VooDo.WinUI.Xaml;

namespace VooDo.WinUI.Core
{

    public interface ITargetProvider
    {

        Target? GetTarget(XamlInfo _xamlInfo);

    }

}
