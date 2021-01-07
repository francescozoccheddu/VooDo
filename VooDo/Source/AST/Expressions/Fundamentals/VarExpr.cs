using System;
using System.Collections.Generic;

using VooDo.Runtime;
using VooDo.Runtime.Controllers;
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
            Eval eval = Controller ? new Eval(_env[Name].Controller) : _env[Name].Eval;
            _env.Script.HookManager.Subscribe(this, new Eval(_env), Name);
            return eval;
        }

        internal sealed override void Assign(Env _env, Eval _value)
        {
            if (Controller)
            {
                if (_value.Value is IControllerFactory factory)
                {
                    _env[Name].SetController(factory);
                }
                else
                {
                    throw new Exception("Not a controller");
                }
            }
            else
            {
                _env[Name].Eval = _value;
            }
        }

        public sealed override int Precedence => 0;

        public sealed override string Code => $"{(Controller ? "$" : "")}{Name}";

        public override void Unsubscribe(HookManager _hookManager) => _hookManager.Unsubscribe(this);

        internal override HashSet<Name> GetVariables() => new HashSet<Name>() { Name };

        #endregion

        #region ASTBase

        public sealed override bool Equals(object _obj)
            => _obj is VarExpr expr && Name.Equals(expr.Name) && Controller.Equals(expr.Controller);

        public sealed override int GetHashCode()
            => Name.GetHashCode();

        #endregion

    }

}
