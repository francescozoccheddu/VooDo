
using VooDo.AST;

namespace VooDo.Runtime.Meta
{
    public interface IMemberProvider
    {

        object EvaluateMember(Name _name, HookManager _hookManager);

    }
}
