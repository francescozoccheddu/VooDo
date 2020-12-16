using VooDo.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VooDo.AST
{

    public abstract class ASTBase
    {

        public abstract string Code { get; }

        #region Object

        public static bool operator ==(ASTBase _left, ASTBase _right) => Identity.AreEqual(_left, _right);
        public static bool operator !=(ASTBase _left, ASTBase _right) => Identity.AreEqual(_left, _right);

        public abstract override bool Equals(object _obj);
        public abstract override int GetHashCode();
        public sealed override string ToString() => Code;

        #endregion

    }

}
