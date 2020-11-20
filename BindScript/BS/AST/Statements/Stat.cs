
using BS.Runtime;

namespace BS.AST.Statements
{
    public abstract class Stat : ASTBase
    {

        internal abstract void Run(Env _env);

    }
}
