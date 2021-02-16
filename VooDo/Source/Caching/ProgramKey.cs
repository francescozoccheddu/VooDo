﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using VooDo.AST;
using VooDo.AST.Names;
using VooDo.Compiling;
using VooDo.Hooks;

namespace VooDo.Caching
{

    public readonly struct ProgramKey : IEquatable<ProgramKey>
    {

        public static ProgramKey Create(Compilation _compilation)
            => Create(_compilation.Script, _compilation.Options);

        public static ProgramKey Create(Script _script, CompilationOptions _options)
            => Create(_script.ToString(), _options);

        public static ProgramKey Create(string _script, CompilationOptions _options)
            => Create(_script, _options.References, _options.ReturnType, _options.HookInitializerProvider);

        public static ProgramKey Create(Script _script, IEnumerable<Reference> _references, ComplexType? _returnType, IHookInitializerProvider _hookInitializerProvider)
            => Create(_script.ToString(), _references, _returnType, _hookInitializerProvider);

        public static ProgramKey Create(string _script, IEnumerable<Reference> _references, ComplexType? _returnType, IHookInitializerProvider _hookInitializerProvider)
            => new ProgramKey(_script, _references, _returnType, _hookInitializerProvider);

        private ProgramKey(string _source, IEnumerable<Reference> _references, ComplexType? _returnType, IHookInitializerProvider _hookInitializerProvider)
        {
            m_source = _source.Trim();
            m_references = Reference.Merge(_references).ToImmutableHashSet();
            m_returnType = _returnType?.ToString();
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

        public CompilationOptions Options { get; }
        public Script Script { get; }

        private readonly int m_hashCode;
        private readonly string m_source;
        private readonly ImmutableHashSet<Reference> m_references;
        private readonly string? m_returnType;
        private readonly IHookInitializerProvider m_hookInitializerProvider;

        public override bool Equals(object? _obj) => _obj is ProgramKey key && Equals(key);
        public bool Equals(ProgramKey _other) =>
            m_hashCode == _other.m_hashCode
            && m_source == _other.m_source
            && m_references.SetEquals(_other.m_references)
            && m_returnType == _other.m_returnType
            && m_hookInitializerProvider.Equals(_other.m_hookInitializerProvider);
        public override int GetHashCode() => m_hashCode;

        public static bool operator ==(ProgramKey? _left, ProgramKey? _right) => _left.Equals(_right);
        public static bool operator !=(ProgramKey? _left, ProgramKey? _right) => !(_left == _right);

    }


}
