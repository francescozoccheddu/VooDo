using VooDo.WinUI.Interfaces;
using VooDo.WinUI.Xaml;

namespace VooDo.WinUI.Components
{

    public sealed class SimpleTargetProvider : ITargetProvider<SimpleTarget>
    {

        public SimpleTarget? GetTarget(XamlInfo _xamlInfo)
            => throw new System.NotImplementedException();

    }

}
