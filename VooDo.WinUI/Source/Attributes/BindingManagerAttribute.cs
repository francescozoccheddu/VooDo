using System.Collections.Generic;

using VooDo.WinUI.Interfaces;

namespace VooDo.WinUI.Attributes
{

    public sealed class BindingManagerAttribute : SingletonTypeTagAttribute
    {

        internal static IBindingManager? Resolve(IEnumerable<Application> _applications)
        {
            if (Resolve(_applications, out Application application))
            {
                return Instantiate<IBindingManager>(application.Target);
            }
            else
            {
                return null;
            }
        }

    }

}
