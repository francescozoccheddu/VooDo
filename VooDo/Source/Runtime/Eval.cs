using System;

using VooDo.Utils;

namespace VooDo.Runtime
{

    public readonly struct Eval
    {

        public Eval(object _value) : this(_value, _value?.GetType())
        { }

        public Eval(object _value, Type _type)
        {
            if (_value != null)
            {
                Ensure.NonNull(_type, nameof(_type));
                if (_value != null && !_type.IsAssignableFrom(_value.GetType()))
                {
                    throw new ArgumentException("Type mismatch", nameof(_type));
                }
            }
            Type = _type;
            Value = _value;
        }

        public Type Type { get; }
        public object Value { get; }

        public override bool Equals(object _obj)
            => _obj is Eval eval && Identity.AreEqual(Value, eval.Value) && Identity.AreEqual(Type, eval.Type);

        public override int GetHashCode() => Identity.CombineHash(Type, Value);

        public static bool operator ==(Eval _a, Eval _b) => _a.Equals(_b);
        public static bool operator !=(Eval _a, Eval _b) => !(_a == _b);
    }

}
