using System;

using VooDo.AST;
using VooDo.Runtime;

namespace VooDo.WinUI
{

    public class Binding : IEquatable<Binding?>
    {

        internal Binding(Script _source, Target _target, Program _program)
        {
            Source = _source;
            Target = _target;
            Program = _program;
        }

        public Script Source { get; }
        public virtual Target Target { get; }
        public virtual Program Program { get; }


        public override bool Equals(object? _obj) => Equals(_obj as Binding);
        public bool Equals(Binding? _other) => ReferenceEquals(this, _other);
        public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);

        public static bool operator ==(Binding? _left, Binding? _right) => ReferenceEquals(_left, _right);
        public static bool operator !=(Binding? _left, Binding? _right) => !(_left == _right);

    }

    public sealed class Binding<TValue> : Binding
    {

        internal Binding(Script _source, Target<TValue> _target, Program<TValue> _program) : base(_source, _target, _program)
        {
        }

        public override Target<TValue> Target => (Target<TValue>) base.Target;
        public override Program<TValue> Program => (Program<TValue>) base.Program;

    }

}
