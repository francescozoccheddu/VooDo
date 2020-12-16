
using VooDo.Runtime;

namespace VooDo.AST.Statements
{
    public abstract class Stat : ASTBase
    {

        internal abstract void Run(Env _env);

    }
}
