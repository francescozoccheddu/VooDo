using System;

using VooDo.Runtime;
using VooDo.Source.Runtime;
using VooDo.Utils;

namespace VooDo.AST.Expressions.Fundamentals
{

    public sealed class VarExpr : Expr
    {

        internal VarExpr(Name _name, bool _controller)
        {
            Ensure.NonNull(_name, nameof(_name));
            Name = _name;
            Controller = _controller;
        }

        public Name Name { get; }
        public bool Controller { get; }

        #region Expr

        internal sealed override object Evaluate(Runtime.Env _env) => Controller ? _env[Name].Controller : _env[Name].Value;

        internal sealed override void Assign(Runtime.Env _env, object _value)
        {
            if (Controller)
            {
                if (_value is IController controller)
                {
                    _env[Name, true].Controller = controller;
                }
                else if (_value is IControllerFactory factory)
                {
                    _env[Name, true].UpdateController(factory);
                }
                else
                {
                    throw new Exception("Not a controller");
                }
            }
            else
            {
                _env[Name, true].Value = _value;
            }
        }

        public sealed override int Precedence => 0;

        public sealed override string Code => $"{(Controller ? "$" : "")}{Name}";


        #endregion

        #region ASTBase

        public sealed override bool Equals(object _obj)
            => _obj is VarExpr expr && Name.Equals(expr.Name) && Controller.Equals(expr.Controller);

        public sealed override int GetHashCode()
            => Name.GetHashCode();

        #endregion

    }

}
