using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Hooks;

namespace VooDo.WinUI.Options
{

    public sealed class HookInitializerAttribute : CombinableTypeTagAttribute
    {

        internal static IHookInitializer? Resolve(IEnumerable<Application> _applications)
        {
            if (Resolve(_applications, out ImmutableArray<Application> resolvedApplications, out EKind kind))
            {
                if (kind == EKind.Combinable)
                {
                    return new HookInitializerList(resolvedApplications.Select(_a => Instantiate<IHookInitializer>(_a.GetType())));
                }
                else
                {
                    return Instantiate<IHookInitializer>(resolvedApplications.Single().GetType());
                }
            }
            else
            {
                return null;
            }
        }

    }

}
