﻿
using System;
using System.Collections.Immutable;

using VooDo.AST.Names;
using VooDo.Compiling;
using VooDo.Utils;

namespace VooDo.WinUI.Options
{

    public sealed class UnresolvedType
    {

        private readonly ComplexType? m_resolved;
        private readonly Type? m_unresolved;

        public static implicit operator UnresolvedType(Type _type) => new UnresolvedType(_type);
        public static implicit operator UnresolvedType(ComplexType _type) => new UnresolvedType(_type);

        public UnresolvedType(Type _type)
        {
            m_unresolved = _type;
        }

        public UnresolvedType(ComplexType _type)
        {
            m_resolved = _type;
        }

        internal ComplexType Resolve(ImmutableArray<Reference> _references)
        {
            ComplexType type = m_resolved ?? ComplexType.FromType(m_unresolved!, true);
            return TypeAliasResolver.Resolve(type, _references);
        }

    }

}