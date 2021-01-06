using System;

namespace VooDo.Runtime.Meta
{
    public interface IGeneric
    {

        Eval Specialize(Env _env, Type[] _arguments);

    }

}
