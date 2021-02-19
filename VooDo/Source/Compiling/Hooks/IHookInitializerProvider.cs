using Microsoft.CodeAnalysis;

namespace VooDo.Hooks
{
    public interface IHookInitializerProvider
    {

        IHookInitializer? Provide(ISymbol _symbol);

    }

}
