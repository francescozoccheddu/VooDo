using System;

using VooDo.Factory.Syntax;
using VooDo.Utils;

namespace VooDo.Factory
{

    public sealed class Global : IEquatable<Global>
    {

        public Global(QualifiedTypeOrVar _type, Identifier _name)
        {
            if (_type is null)
            {
                throw new ArgumentNullException(nameof(_type));
            }
            if (_name is null)
            {
                throw new ArgumentNullException(nameof(_name));
            }
            Name = _name;
            Type = _type;
        }

        public Identifier Name { get; }
        public QualifiedTypeOrVar Type { get; }

        public override bool Equals(object _obj) => Equals(_obj as Global);
        public bool Equals(Global _other) => _other is not null && Name == _other.Name && Type == _other.Type;
        public static bool operator ==(Global _left, Global _right) => Identity.AreEqual(_left, _right);
        public static bool operator !=(Global _left, Global _right) => !(_left == _right);
        public override int GetHashCode() => Identity.CombineHash(Name, Type);
        public override string ToString() => $"{{{nameof(Global)}: {Type} {Name}}}";

        public Global WithType(QualifiedTypeOrVar _type)
            => _type == Type ? this : new Global(_type, Name);

        public Global WithName(Identifier _name)
            => _name == Name ? this : new Global(Type, _name);

    }

}
