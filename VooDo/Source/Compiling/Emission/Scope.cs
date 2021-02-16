﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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

            public GlobalDefinition(GlobalPrototype _global, string _identifier) : this(_global, SyntaxFactory.Identifier(_identifier)) { }

            public GlobalDefinition(GlobalPrototype _global, SyntaxToken _identifier)
            {
                Prototype = _global;
                Identifier = _identifier;
            }

            public GlobalPrototype Prototype { get; }
            public SyntaxToken Identifier { get; }

        }

        private readonly Dictionary<string, bool> m_names;
        private readonly List<GlobalDefinition> m_globals;

        internal Scope() : this(new List<GlobalDefinition>(), new Dictionary<string, bool>())
        { }

        private Scope(List<GlobalDefinition> _globals, Dictionary<string, bool> _names)
        {
            m_names = _names;
            m_globals = _globals;
        }

        public bool IsNameTaken(Identifier _name)
            => m_names.ContainsKey(_name);

        public bool IsGlobal(Identifier _name)
            => m_names.TryGetValue(_name, out bool isGlobal) && isGlobal;

        public ImmutableArray<GlobalDefinition> GetGlobalDefinitions()
            => m_globals.ToImmutableArray();

        private static GlobalDefinition CreateGlobalDefinition(GlobalPrototype _global, int _index)
            => new GlobalDefinition(_global, CompilationConstants.globalFieldPrefix + _index);

        public void AddLocal(Node _source, Identifier _name)
        {
            if (IsNameTaken(_name))
            {
                throw new VariableRedefinitionProblem(_source, _name).AsThrowable();
            }
            m_names.Add(_name, false);
        }

        public GlobalDefinition AddGlobal(GlobalPrototype _global)
        {
            if (_global.Global.Name is not null)
            {
                if (IsNameTaken(_global.Global.Name))
                {
                    throw new VariableRedefinitionProblem(_global.Source, _global.Global.Name).AsThrowable();
                }
                m_names.Add(_global.Global.Name, true);
            }
            GlobalDefinition definition = CreateGlobalDefinition(_global, m_globals.Count);
            m_globals.Add(definition);
            return definition;
        }

        internal Scope CreateNested() => new Scope(m_globals, new Dictionary<string, bool>(m_names));

    }

}