using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.WinUI.Interfaces;

namespace VooDo.WinUI.Attributes
{

    public sealed class TargetProviderAttribute : CombinableTypeTagAttribute
    {

        internal static ITargetProvider<TTarget>? Resolve<TTarget>(IEnumerable<Application> _applications) where TTarget : ITarget
        {
            if (Resolve(_applications, out ImmutableArray<Application> resolvedApplications, out EKind kind))
            {
                if (kind == EKind.Combinable)
                {
                    return new TargetProviderList<TTarget>(resolvedApplications.Select(_a => Instantiate<ITargetProvider<TTarget>>(_a.GetType())));
                }
                else
                {
                    return Instantiate<ITargetProvider<TTarget>>(resolvedApplications.Single().GetType());
                }
            }
            else
            {
                return null;
            }
        }

    }

}
