
using VooDo.AST;

namespace VooDo.Runtime.Meta
{
    public interface IMemberProvider
    {

        Eval EvaluateMember(Env _env, Name _name);

    }
}
