using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using VooDo.AST;
using VooDo.AST.Names;
using VooDo.Compiling;
using VooDo.Hooks;
using VooDo.Utils;

namespace VooDo.Caching
{

    public readonly struct LoaderKey : IEquatable<LoaderKey>
    {

        public static LoaderKey Create(Compilation _compilation)
            => Create(_compilation.Script, _compilation.Options);

        public static LoaderKey Create(Script _script, Options _options)
            => Create(_script, _options);

        public static LoaderKey Create(Script _script, IEnumerable<Reference> _references, ComplexType? _returnType, IHookInitializer _hookInitializer)
            => new LoaderKey(_script, _references, _returnType, _hookInitializer);

        private LoaderKey(Script _script, IEnumerable<Reference> _references, ComplexType? _returnType, IHookInitializer _hookInitializer)
        {
            Script = _script;
            scriptCode = _script.ToString();
            References = Reference.Merge(_references).ToImmutableHashSet();
            ReturnType = _returnType;
            returnTypeCode = _returnType?.ToString();
            HookInitializer = _hookInitializer;
            m_hashCode = Identity.CombineHash(References.Count, scriptCode, returnTypeCode, HookInitializer);
        }

        public Options CreateMatchingOptions() => Options.Default with
        {
            HookInitializer = HookInitializer,
            References = References.ToImmutableArray(),
            ReturnType = ReturnType
        };

        private readonly int m_hashCode;
        internal readonly string scriptCode;
        internal readonly string? returnTypeCode;

        public Script Script { get; }
        public ImmutableHashSet<Reference> References { get; }
        public ComplexType? ReturnType { get; }
        public IHookInitializer HookInitializer { get; }

        public override bool Equals(object? _obj) => _obj is LoaderKey key && Equals(key);
        public bool Equals(LoaderKey _other) =>
            m_hashCode == _other.m_hashCode
            && scriptCode == _other.scriptCode
            && returnTypeCode == _other.returnTypeCode
            && References.SetEquals(_other.References)
            && HookInitializer.Equals(_other.HookInitializer);
        public override int GetHashCode() => m_hashCode;

        public static bool operator ==(LoaderKey? _left, LoaderKey? _right) => _left.Equals(_right);
        public static bool operator !=(LoaderKey? _left, LoaderKey? _right) => !(_left == _right);

    }

}
