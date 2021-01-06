using VooDo.Utils;

namespace VooDo.AST
{

    public abstract class ASTBase
    {

        public abstract string Code { get; }

        public abstract void Unsubscribe();

        #region Object

        public static bool operator ==(ASTBase _left, ASTBase _right) => Identity.AreEqual(_left, _right);
        public static bool operator !=(ASTBase _left, ASTBase _right) => !(_left == _right);

        public abstract override bool Equals(object _obj);
        public abstract override int GetHashCode();
        public sealed override string ToString() => Code;

        #endregion

    }

}
