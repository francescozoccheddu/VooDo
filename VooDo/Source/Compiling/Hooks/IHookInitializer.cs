using Microsoft.CodeAnalysis;

using VooDo.AST.Expressions;

namespace VooDo.Hooks
{
    public interface IHookInitializer
    {

        Expression? GetInitializer(ISymbol _symbol);

    }

}
