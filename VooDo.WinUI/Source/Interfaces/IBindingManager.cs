using System.Collections.Generic;

using VooDo.WinUI.Xaml;

namespace VooDo.WinUI.Interfaces
{

    public interface IBindingManager
    {

        Binding AddBinding(XamlInfo _xamlInfo);
        void RemoveBinding(Binding _binding);
        void ClearBindings();
        IEnumerable<Binding> Bindings { get; }

    }

}
