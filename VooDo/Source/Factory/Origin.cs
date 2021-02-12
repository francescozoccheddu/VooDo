


using System;

using VooDo.Utils;

namespace VooDo.Factory
{

    public readonly struct Origin : IEquatable<Origin>
    {

        public enum EKind
        {
            Unknown = 0, Source = 1, HookInitializer = 2
        }

        public static Origin Unknown { get; } = default;

        internal static Origin HookInitializer { get; } = new Origin(EKind.HookInitializer, -1, 0);

        internal static Origin FromSource(int _start, int _length)
        {
            if (_start < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_start));
            }
            if (_length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_length));
            }
            return new Origin(EKind.Source, _start, _length);
        }


        public EKind Kind { get; }
        public int Start { get; }
        public int Length { get; }
        public int End => Start + End;

        private Origin(EKind _kind, int _start, int _length)
        {
            Kind = _kind;
            Start = _start;
            Length = _length;
        }

        public override bool Equals(object? _obj) => _obj is Origin origin && Equals(origin);
        public bool Equals(Origin _other) => Kind == _other.Kind && Start == _other.Start && Length == _other.Length;
        public static bool operator ==(Origin? _left, Origin? _right) => Identity.AreEqual(_left, _right);
        public static bool operator !=(Origin? _left, Origin? _right) => !(_left == _right);
        public override string ToString() => Kind switch
        {
            EKind.Source => $"Source {Start}..{End}",
            EKind.HookInitializer => "HookInitializer",
            _ => "Unknown"
        };
        public override int GetHashCode() => Identity.CombineHash(Kind, Start, Length);

    }

}
