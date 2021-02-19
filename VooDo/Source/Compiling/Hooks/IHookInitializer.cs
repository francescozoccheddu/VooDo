using VooDo.AST.Expressions;

namespace VooDo.Hooks
{

    public interface IHookInitializer
    {

        Expression CreateInitializer();

    }

}
