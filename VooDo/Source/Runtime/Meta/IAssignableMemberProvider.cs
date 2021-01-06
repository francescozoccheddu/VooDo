
using VooDo.AST;

namespace VooDo.Runtime.Meta
{
    public interface IAssignableMemberProvider : IMemberProvider
    {

        void AssignMember(Env _env, Name _name, Eval _eval);

    }
}
