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

        internal sealed override Eval Evaluate(Env _env)
        {
            Eval eval = new Eval(Controller ? _env[Name].Controller : _env[Name].Value); //TODO Type
            _env.Script.HookManager.Subscribe(this, new Eval(_env), Name);
            return eval;
        }

        internal sealed override void Assign(Env _env, Eval _value)
        {
            // TODO Type
            if (Controller)
            {
                if (_value.Value is IController controller)
                {
                    _env[Name, true].Controller = controller;
                }
                else if (_value.Value is IControllerFactory factory)
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
                _env[Name, true].Value = _value.Value;
            }
        }

        public sealed override int Precedence => 0;

        public sealed override string Code => $"{(Controller ? "$" : "")}{Name}";

        public override void Unsubscribe(HookManager _hookManager) => _hookManager.Unsubscribe(this);

        #endregion

        #region ASTBase

        public sealed override bool Equals(object _obj)
            => _obj is VarExpr expr && Name.Equals(expr.Name) && Controller.Equals(expr.Controller);

        public sealed override int GetHashCode()
            => Name.GetHashCode();

        #endregion

    }

}
