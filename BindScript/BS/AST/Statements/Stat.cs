
using BS.Runtime;

namespace BS.AST.Statements
{
    public abstract class Stat : Syntax.ICode
    {

        public abstract string Code { get; }

        internal abstract void Run(Env _env);

    }
}
