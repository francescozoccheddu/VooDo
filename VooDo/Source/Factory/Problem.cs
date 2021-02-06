

using Microsoft.CodeAnalysis;

using System;

using VooDo.Utils;

namespace VooDo.Factory
{

    public readonly struct Problem : IEquatable<Problem>
    {

        public enum ESeverity
        {
            Error, Warning
        }

        internal Problem(Origin _origin, ESeverity _severity, Diagnostic _diagnostic)
        {
            Origin = _origin;
            Severity = _severity;
            Diagnostic = _diagnostic;
        }

        public Origin Origin { get; }
        public ESeverity Severity { get; }
        internal Diagnostic Diagnostic { get; }

        public string Message => Diagnostic.GetMessage();

        public override bool Equals(object? _obj) => _obj is Problem problem && Equals(problem);
        public bool Equals(Problem _other) => Origin.Equals(_other.Origin) && Severity == _other.Severity && Diagnostic.Equals(_other.Diagnostic) && Message == _other.Message;
        public static bool operator ==(Problem _left, Problem _right) => _left.Equals(_right);
        public static bool operator !=(Problem _left, Problem _right) => !(_left == _right);
        public override int GetHashCode() => Identity.CombineHash(Origin, Severity, Diagnostic);

    }

}
