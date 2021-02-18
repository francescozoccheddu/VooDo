using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Hooks;

namespace VooDo.WinUI.Attributes
{

    public sealed class HookInitializerProviderAttribute : CombinableTypeTagAttribute
    {

        internal static IHookInitializerProvider? Resolve(IEnumerable<Application> _applications)
        {
            if (Resolve(_applications, out ImmutableArray<Application> resolvedApplications, out EKind kind))
            {
                if (kind == EKind.Combinable)
                {
                    return new HookInitializerList(resolvedApplications.Select(_a => Instantiate<IHookInitializerProvider>(_a.GetType())));
                }
                else
                {
                    return Instantiate<IHookInitializerProvider>(resolvedApplications.Single().GetType());
                }
            }
            else
            {
                return null;
            }
        }

    }

}
