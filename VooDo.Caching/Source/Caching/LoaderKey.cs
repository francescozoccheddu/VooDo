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

        public static LoaderKey Create(Script _script, CompilationOptions _options)
            => Create(_script, _options);

        public static LoaderKey Create(Script _script, IEnumerable<Reference> _references, ComplexType? _returnType, IHookInitializerProvider _hookInitializerProvider)
            => new LoaderKey(_script, _references, _returnType, _hookInitializerProvider);

        private LoaderKey(Script _script, IEnumerable<Reference> _references, ComplexType? _returnType, IHookInitializerProvider _hookInitializerProvider)
        {
            Script = _script;
            scriptCode = _script.ToString();
            References = Reference.Merge(_references).ToImmutableHashSet();
            ReturnType = _returnType;
            returnTypeCode = _returnType?.ToString();
            HookInitializerProvider = _hookInitializerProvider;
            m_hashCode = Identity.CombineHash(References.Count, scriptCode, returnTypeCode, HookInitializerProvider);
        }

        public CompilationOptions CreateMatchingOptions() => CompilationOptions.Default with
        {
            HookInitializerProvider = HookInitializerProvider,
            References = References.ToImmutableArray(),
            ReturnType = ReturnType
        };

        private readonly int m_hashCode;
        internal readonly string scriptCode;
        internal readonly string? returnTypeCode;

        public Script Script { get; }
        public ImmutableHashSet<Reference> References { get; }
        public ComplexType? ReturnType { get; }
        public IHookInitializerProvider HookInitializerProvider { get; }

        public override bool Equals(object? _obj) => _obj is LoaderKey key && Equals(key);
        public bool Equals(LoaderKey _other) =>
            m_hashCode == _other.m_hashCode
            && scriptCode == _other.scriptCode
            && returnTypeCode == _other.returnTypeCode
            && References.SetEquals(_other.References)
            && HookInitializerProvider.Equals(_other.HookInitializerProvider);
        public override int GetHashCode() => m_hashCode;

        public static bool operator ==(LoaderKey? _left, LoaderKey? _right) => _left.Equals(_right);
        public static bool operator !=(LoaderKey? _left, LoaderKey? _right) => !(_left == _right);

    }

}
