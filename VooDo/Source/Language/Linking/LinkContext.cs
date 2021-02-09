namespace VooDo.Language.Linking
{

    internal sealed record LinkContext(Scope Scope, Bundle.Dealer BundleDealer, EventContext? EventContext = null, HookContext? HookContext = null)
    {

    }

}
