using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VooDo.Transformation
{
    public interface IHookInitializer
    {

        ExpressionSyntax GetHookInitializerSyntax();

    }

}
