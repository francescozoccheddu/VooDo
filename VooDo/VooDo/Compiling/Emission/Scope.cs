﻿using Microsoft.CodeAnalysis;

using System.Collections.Generic;
using System.Collections.Immutable;

using VooDo.AST;
using VooDo.AST.Names;
using VooDo.Problems;
using VooDo.Utils;

namespace VooDo.Compiling.Emission
{

    internal sealed class Scope
    {

        internal sealed class GlobalDefinition
        {

            internal GlobalDefinition(GlobalPrototype _global, int _index)
            {
                Prototype = _global;
                Identifier = SyntaxFactoryUtils.Identifier(Identifiers.globalPrefix + _index);
            }

            internal GlobalPrototype Prototype { get; }
            internal SyntaxToken Identifier { get; }

        }

        private enum EKind
        {
            Local, GlobalVariable, GlobalConst
        }

        private readonly Dictionary<string, EKind> m_names;
        private readonly List<GlobalDefinition> m_globals;

        internal Scope() : this(new List<GlobalDefinition>(), new Dictionary<string, EKind>())
        { }

        private Scope(List<GlobalDefinition> _globals, Dictionary<string, EKind> _names)
        {
            m_names = _names;
            m_globals = _globals;
        }

        public bool IsConstant(Identifier _name)
            => m_names.TryGetValue(_name, out EKind kind) && kind == EKind.GlobalConst;

        public bool IsGlobal(Identifier _name)
            => m_names.TryGetValue(_name, out EKind kind) && kind != EKind.Local;

        public bool IsNameTaken(Identifier _name)
            => m_names.ContainsKey(_name);

        public ImmutableArray<GlobalDefinition> GetGlobalDefinitions()
            => m_globals.ToImmutableArray();

        public void AddLocal(Node _source, Identifier _name)
        {
            if (IsNameTaken(_name))
            {
                throw new VariableRedefinitionProblem(_source, _name).AsThrowable();
            }
            m_names.Add(_name, EKind.Local);
        }

        public GlobalDefinition AddGlobal(GlobalPrototype _global)
        {
            if (_global.Global.Name is not null)
            {
                if (IsNameTaken(_global.Global.Name))
                {
                    throw new VariableRedefinitionProblem(_global.Source, _global.Global.Name).AsThrowable();
                }
                m_names.Add(_global.Global.Name, _global.Global.IsConstant ? EKind.GlobalConst : EKind.GlobalVariable);
            }
            GlobalDefinition definition = new GlobalDefinition(_global, m_globals.Count);
            m_globals.Add(definition);
            return definition;
        }

        internal Scope CreateNested() => new Scope(m_globals, new Dictionary<string, EKind>(m_names));

    }

}
