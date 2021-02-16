namespace VooDo.WinUI
{

    public interface ITargetProvider
    {

        Target? GetTarget(object _targetPrototype);

    }

    public sealed class TargetProviderAttribute : CombinableServiceProviderAttribute
    {

        internal static (ITargetProvider provider, int priority)? GetTargetProvider()
            => GetProvider<ITargetProvider, TargetProviderAttribute>(_h => new TargetProviderList(_h));

    }

}
