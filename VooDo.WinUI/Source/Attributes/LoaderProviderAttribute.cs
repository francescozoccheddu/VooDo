using System.Collections.Generic;

using VooDo.WinUI.Interfaces;

namespace VooDo.WinUI.Attributes
{

    public sealed class LoaderProviderAttribute : SingletonTypeTagAttribute
    {

        internal static ILoaderProvider<TTarget>? Resolve<TTarget>(IEnumerable<Application> _applications) where TTarget : ITarget
        {
            if (Resolve(_applications, out Application application))
            {
                return Instantiate<ILoaderProvider<TTarget>>(application.Target);
            }
            else
            {
                return null;
            }
        }

    }

}
