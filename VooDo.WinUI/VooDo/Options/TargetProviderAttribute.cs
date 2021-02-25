using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.WinUI.Components;
using VooDo.WinUI.Core;

namespace VooDo.WinUI.Options
{

    public sealed class TargetProviderAttribute : CombinableTypeTagAttribute
    {

        internal static ITargetProvider? Resolve(IEnumerable<Application> _applications)
        {
            if (Resolve(_applications, out ImmutableArray<Application> resolvedApplications, out EKind kind))
            {
                if (kind == EKind.Combinable)
                {
                    return new TargetProviderList(resolvedApplications.Select(_a => Instantiate<ITargetProvider>(_a.GetType())));
                }
                else
                {
                    return Instantiate<ITargetProvider>(resolvedApplications.Single().GetType());
                }
            }
            else
            {
                return null;
            }
        }

    }

}
