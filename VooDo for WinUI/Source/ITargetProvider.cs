namespace VooDo.WinUI
{

    public interface ITargetProvider
    {

        Target? GetTarget(object _targetPrototype);

    }

    public sealed class TargetProviderAttribute : ServiceProviderAttribute
    {

        public bool CanCooperate { get; set; }

    }

}
