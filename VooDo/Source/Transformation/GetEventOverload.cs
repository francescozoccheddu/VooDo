
using System;
using System.Collections.Generic;
using System.Linq;

using VooDo.Utils;

namespace VooDo.Transformation
{

    public readonly struct GetEventOverload
    {

        public GetEventOverload(IEnumerable<EArgumentType> _arguments)
        {
            if (_arguments is null)
            {
                throw new ArgumentNullException(nameof(_arguments));
            }
            Arguments = _arguments.ToArray();
            string suffix = Arguments.Count == 0 ? "Empty" : string.Concat(_arguments.Select(_a => _a == EArgumentType.Out ? "Out" : "Ref"));
            Identifier = string.Format(Identifiers.getEventMethodFormat, suffix);
        }

        public enum EArgumentType
        {
            Ref, Out
        }

        public IReadOnlyList<EArgumentType> Arguments { get; }
        public string Identifier { get; }

        public override bool Equals(object _obj) => _obj is GetEventOverload def && def.Arguments.SequenceEqual(Arguments);
        public override int GetHashCode() => Identity.CombineHashes(Arguments);
        public static bool operator ==(GetEventOverload _left, GetEventOverload _right) => _left.Equals(_right);
        public static bool operator !=(GetEventOverload _left, GetEventOverload _right) => !_left.Equals(_right);

    }

}
