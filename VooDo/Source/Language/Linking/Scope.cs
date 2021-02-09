using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using System.Collections.Generic;
using System.Collections.Immutable;

using VooDo.Language.AST.Names;
using VooDo.Utils;

namespace VooDo.Language.Linking
{

    internal sealed class Scope
    {

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

        private Scope() : this(new List<GlobalDefinition>(), new Dictionary<string, bool>())
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
            => new GlobalDefinition(_global, $"global_{_index}");

        public GlobalDefinition AddGlobal(Global _global)
        {
            GlobalDefinition definition = CreateGlobalDefinition(_global, m_globals.Count);
            m_globals.Add(definition);
            return definition;
        }

        public ImmutableArray<GlobalDefinition> AddGlobals(IEnumerable<Global> _globals)
        {
            ImmutableArray<GlobalDefinition> definitions = _globals.SelectIndexed((_g, _i) => CreateGlobalDefinition(_g, m_globals.Count + _i)).ToImmutableArray();
            m_globals.AddRange(definitions);
            return definitions;
        }

        internal Scope CreateNested() => new Scope(m_globals, new Dictionary<string, bool>(m_names));

    }

}
