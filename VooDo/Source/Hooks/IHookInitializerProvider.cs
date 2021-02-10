using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VooDo.Hooks
{
    public interface IHookInitializerProvider
    {

        IHookInitializer GetHookInitializer(MemberAccessExpressionSyntax _syntax, SemanticModel _semantics);
        IHookInitializer GetHookInitializer(ElementAccessExpressionSyntax _syntax, SemanticModel _semantics);

    }

}
