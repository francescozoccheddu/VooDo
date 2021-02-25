using Microsoft.CodeAnalysis;

using System.Collections.Immutable;

using VooDo.AST.Expressions;
using VooDo.Compiling;

namespace VooDo.Hooks
{
    public interface IHookInitializer
    {

        Expression? GetInitializer(ISymbol _symbol, ImmutableArray<Reference> _references);

    }

}
