using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

using VooDo.Runtime.Implementation;

namespace VooDo.Runtime
{

    public sealed class Loader : IEquatable<Loader?>
    {

        private abstract class SpyProgram : TypedProgram
        {
            internal const string tagPrefix = __VooDo_Reserved_tagPrefix;
            private SpyProgram()
            { }
        }

        public static Loader FromType(Type _type)
            => new(_type);

        public static Loader FromAssembly(Assembly _assembly)
            => new(_assembly.GetTypes().Single(_t => _t.IsSubclassOf(typeof(Program))));

        private readonly Type m_type;

        public Type ReturnType { get; }
        public bool IsTyped => ReturnType != typeof(void);

        private Loader(Type _type)
        {
            if (!_type.IsSubclassOf(typeof(Program)))
            {
                throw new ArgumentException("Not a Program", nameof(_type));
            }
            if (_type.GetConstructor(Type.EmptyTypes) is null)
            {
                throw new ArgumentException("Type does not have a parameterless constructor", nameof(_type));
            }
            m_type = _type;
            ReturnType = m_type.BaseType == typeof(Program)
                ? typeof(void)
                : m_type.BaseType.GenericTypeArguments.Single();
            Tags = m_type
                .GetFields(BindingFlags.Static | BindingFlags.NonPublic)
                .Where(_f => _f.IsLiteral && _f.Name.StartsWith(SpyProgram.tagPrefix))
                .ToImmutableDictionary(_f => _f.Name.Substring(SpyProgram.tagPrefix.Length), _f => _f.GetRawConstantValue())!;
        }

        public IProgram Create()
        {
            Program program = (Program)Activator.CreateInstance(m_type)!;
            program.loader = this;
            return program;
        }

        public ITypedProgram<TReturn> Create<TReturn>()
            => (ITypedProgram<TReturn>)Create();

        #region Tags

        public ImmutableDictionary<string, object?> Tags { get; }

        private bool TryGetTag<TValue>(string _name, out TValue? _value) where TValue : notnull
        {
            if (Tags.TryGetValue(_name, out object? value) && value is null or TValue)
            {
                _value = (TValue?)value;
                return true;
            }
            else
            {
                _value = default!;
                return false;
            }
        }

        private TValue GetTag<TValue>(string _name) where TValue : notnull
            => TryGetTag(_name, out TValue? value) ? value! : throw new KeyNotFoundException();

        public int GetIntTag(string _name) => GetTag<int>(_name);
        public uint GetUIntTag(string _name) => GetTag<uint>(_name);
        public short GetShortTag(string _name) => GetTag<short>(_name);
        public ushort GetUShortTag(string _name) => GetTag<ushort>(_name);
        public long GetLongTag(string _name) => GetTag<long>(_name);
        public ulong GetULongTag(string _name) => GetTag<ulong>(_name);
        public decimal GetDecimalTag(string _name) => GetTag<decimal>(_name);
        public sbyte GetSByteTag(string _name) => GetTag<sbyte>(_name);
        public byte GetByteTag(string _name) => GetTag<byte>(_name);
        public char GetCharTag(string _name) => GetTag<char>(_name);
        public string? GetStringTag(string _name) => GetTag<string>(_name);
        public float GetFloatTag(string _name) => GetTag<float>(_name);
        public double GetDoubleTag(string _name) => GetTag<double>(_name);
        public bool GetBoolTag(string _name) => GetTag<bool>(_name);

        public bool TryGetIntTag(string _name, out int _value) => TryGetTag(_name, out _value);
        public bool TryGetUIntTag(string _name, out uint _value) => TryGetTag(_name, out _value);
        public bool TryGetShortTag(string _name, out short _value) => TryGetTag(_name, out _value);
        public bool TryGetUShortTag(string _name, out ushort _value) => TryGetTag(_name, out _value);
        public bool TryGetLongTag(string _name, out long _value) => TryGetTag(_name, out _value);
        public bool TryGetULongTag(string _name, out ulong _value) => TryGetTag(_name, out _value);
        public bool TryGetDecimalTag(string _name, out decimal _value) => TryGetTag(_name, out _value);
        public bool TryGetSByteTag(string _name, out sbyte _value) => TryGetTag(_name, out _value);
        public bool TryGetByteTag(string _name, out byte _value) => TryGetTag(_name, out _value);
        public bool TryGetCharTag(string _name, out char _value) => TryGetTag(_name, out _value);
        public bool TryGetStringTag(string _name, out string _value) => TryGetTag(_name, out _value);
        public bool TryGetFloatTag(string _name, out float _value) => TryGetTag(_name, out _value);
        public bool TryGetDoubleTag(string _name, out double _value) => TryGetTag(_name, out _value);
        public bool TryGetBoolTag(string _name, out bool _value) => TryGetTag(_name, out _value);

        #endregion

        #region Identity

        public override bool Equals(object? _obj) => Equals(_obj as Loader);
        public bool Equals(Loader? _other) => _other is not null && m_type == _other.m_type;
        public override int GetHashCode() => m_type.GetHashCode();

        public static bool operator ==(Loader? _left, Loader? _right) => _left is not null && _left.Equals(_right);
        public static bool operator !=(Loader? _left, Loader? _right) => !(_left == _right);

        #endregion

    }

}
