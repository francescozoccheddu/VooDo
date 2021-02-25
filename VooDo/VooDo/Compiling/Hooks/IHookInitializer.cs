using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using VooDo.AST.Expressions;

namespace VooDo.Hooks
{
    public interface IHookInitializer
    {

        Expression? GetInitializer(ISymbol _symbol, CSharpCompilation _compilation);

    }

}
