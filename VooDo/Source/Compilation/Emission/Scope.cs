using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Language.AST.Names;

namespace VooDo.Language.Linking
{

    internal sealed class Scope
    {

        private const string c_globalFieldNameFormat = "field_{0}";

        internal sealed class GlobalDefinition
        {

            public GlobalDefinition(Global _global, string _identifier) : this(_global, SyntaxFactory.Identifier(_identifier)) { }

            public GlobalDefinition(Global _global, SyntaxToken _identifier)
            {
                Global = _global;
                Identifier = _identifier;
            }

            public Global Global { get; }
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

        private static GlobalDefinition CreateGlobalDefinition(Global _global, int _index)
            => new GlobalDefinition(_global, string.Format(c_globalFieldNameFormat, _index));

        public void AddLocal(Identifier _name)
        {
            if (IsNameTaken(_name))
            {
                throw new InvalidOperationException($"Redefinition of {_name}");
            }
            m_names.Add(_name, false);
        }

        public GlobalDefinition AddGlobal(Global _global)
        {
            if (_global.Name is not null)
            {
                if (IsNameTaken(_global.Name))
                {
                    throw new InvalidOperationException($"Redefinition of {_global.Name}");
                }
                m_names.Add(_global.Name, true);
            }
            GlobalDefinition definition = CreateGlobalDefinition(_global, m_globals.Count);
            m_globals.Add(definition);
            return definition;
        }

        public ImmutableArray<GlobalDefinition> AddGlobals(IEnumerable<Global> _globals)
            => _globals.Select(AddGlobal).ToImmutableArray();

        internal Scope CreateNested() => new Scope(m_globals, new Dictionary<string, bool>(m_names));

    }

}
