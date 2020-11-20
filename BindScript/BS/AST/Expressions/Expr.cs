
using BS.AST.Expressions.Fundamentals;
using BS.AST.Expressions.Literals;
using BS.AST.Expressions.Operators.Comparisons;
using BS.AST.Expressions.Operators.Operations;
using BS.Exceptions.Runtime.Expressions;
using BS.Runtime;
using BS.Utils;

namespace BS.AST.Expressions
{

    public abstract class Expr : ASTBase, Syntax.IExpr
    {

        internal abstract object Evaluate(Env _env);

        internal virtual void Assign(Env _env, object _value) => new UnassignableError(this);

        #region Syntax.IExpr

        public abstract int Priority { get; }

        #endregion

        #region Meta AST
#if BS_AST_EXPR_META
        public static implicit operator Expr(bool _value) => new BoolLitExpr(_value);
        public static implicit operator Expr(int _value) => new IntLitExpr(_value);
        public static implicit operator Expr(double _value) => new RealLitExpr(_value);
        public static implicit operator Expr(string _value) => new StringLitExpr(_value);
        public static SumOpExpr operator +(Expr _left, Expr _right) => new SumOpExpr(_left, _right);
        public static DiffOpExpr operator -(Expr _left, Expr _right) => new DiffOpExpr(_left, _right);
        public static ProdOpExpr operator *(Expr _left, Expr _right) => new ProdOpExpr(_left, _right);
        public static FractOpExpr operator /(Expr _left, Expr _right) => new FractOpExpr(_left, _right);
        public static AndOpExpr operator &(Expr _left, Expr _right) => new AndOpExpr(_left, _right);
        public static OrOpExpr operator |(Expr _left, Expr _right) => new OrOpExpr(_left, _right);
        public static Expr operator >(Expr _left, Expr _right) => new GtOpExpr(_left, _right);
        public static Expr operator <(Expr _left, Expr _right) => new LtOpExpr(_left, _right);
        public static Expr operator >=(Expr _left, Expr _right) => new GeOpExpr(_left, _right);
        public static Expr operator <=(Expr _left, Expr _right) => new LeOpExpr(_left, _right);
        public static Expr operator ==(Expr _left, Expr _right) => new EqOpExpr(_left, _right);
        public static Expr operator !=(Expr _left, Expr _right) => new NeqOpExpr(_left, _right);
        public static NegOpExpr operator -(Expr _arg) => new NegOpExpr(_arg);
        public static NotOpExpr operator !(Expr _arg) => new NotOpExpr(_arg);
        public CallOpExpr Call(params Expr[] _args) => new CallOpExpr(this, new ArgList(_args));
        public MemberExpr Dot(Name _name) => new MemberExpr(this, _name);
        public CastExpr Cast(Expr _type) => new CastExpr(this, _type);
        public EaseOpExpr Ease(Expr _ease) => new EaseOpExpr(this, _ease);
        public SubscriptExpr this[Expr _index] => new SubscriptExpr(this, _index);
        public abstract override bool Equals(object _obj);
        public abstract override int GetHashCode();
#endif
        #endregion

    }

}
