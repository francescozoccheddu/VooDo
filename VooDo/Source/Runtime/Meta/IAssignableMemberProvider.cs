
using VooDo.AST;

namespace VooDo.Runtime.Meta
{
    public interface IAssignableMemberProvider : IMemberProvider
    {

        void AssignMember(Name _name, object _value);

    }
}
