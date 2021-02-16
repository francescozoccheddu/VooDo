using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using VooDo.AST;
using VooDo.AST.Names;
using VooDo.Compiling;
using VooDo.Hooks;

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
            m_source = _script.ToString();
            m_references = Reference.Merge(_references).ToImmutableHashSet();
            m_returnType = _returnType;
            m_hookInitializerProvider = _hookInitializerProvider;
            m_hashCode = 0;
            foreach (Reference reference in m_references)
            {
                unchecked
                {
                    m_hashCode += reference.GetHashCode();
                }
            }
            m_hashCode = HashCode.Combine(m_hashCode, m_source, m_returnType, m_hookInitializerProvider);
        }

        public CompilationOptions CreateMatchingOptions() => CompilationOptions.Default with
        {
            HookInitializerProvider = m_hookInitializerProvider,
            References = m_references.ToImmutableArray(),
            ReturnType = m_returnType
        };

        public Script Script { get; }

        private readonly int m_hashCode;
        private readonly string m_source;
        private readonly ImmutableHashSet<Reference> m_references;
        private readonly ComplexType? m_returnType;
        private readonly IHookInitializerProvider m_hookInitializerProvider;

        public override bool Equals(object? _obj) => _obj is LoaderKey key && Equals(key);
        public bool Equals(LoaderKey _other) =>
            m_hashCode == _other.m_hashCode
            && m_source == _other.m_source
            && m_references.SetEquals(_other.m_references)
            && m_returnType == _other.m_returnType
            && m_hookInitializerProvider.Equals(_other.m_hookInitializerProvider);
        public override int GetHashCode() => m_hashCode;

        public static bool operator ==(LoaderKey? _left, LoaderKey? _right) => _left.Equals(_right);
        public static bool operator !=(LoaderKey? _left, LoaderKey? _right) => !(_left == _right);

    }


}
