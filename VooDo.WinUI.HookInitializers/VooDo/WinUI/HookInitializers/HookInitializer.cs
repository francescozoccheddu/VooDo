
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using System.Collections.Immutable;
using System.Linq;

using VooDo.AST.Expressions;
using VooDo.AST.Names;
using VooDo.Hooks;
using VooDo.Utils;

namespace VooDo.WinUI.HookInitializers
{

    public abstract class HookInitializer : IHookInitializer
    {

        private const string c_hooksNamespace = "VooDo.WinUI.Hooks";

        private Identifier? m_alias;
        private Compilation? m_cachedCompilation;
        private readonly bool m_canCache;
        private readonly QualifiedType m_type;

        internal HookInitializer() : this(null)
        {
            m_canCache = true;
        }

        internal HookInitializer(Identifier? _alias)
        {
            m_alias = _alias;
            m_type = new(Namespace.Parse(c_hooksNamespace), new Identifier(HookTypeName));
        }

        protected abstract Identifier HookTypeName { get; }

        protected Expression GetInitializer(CSharpCompilation _compilation, params Argument[] _arguments)
        {
            if (m_canCache && m_cachedCompilation != _compilation)
            {
                MetadataReference? reference = ReferenceFinder.FindByNamespace(c_hooksNamespace, _compilation).FirstOrDefault();
                m_alias = reference is null ? null : ReferenceFinder.GetAlias(reference);
                m_cachedCompilation = _compilation;
            }
            return new ObjectCreationExpression(
                m_type with
                {
                    Alias = m_alias ?? "global"
                },
                _arguments.ToImmutableArray());
        }

        public abstract Expression? GetInitializer(ISymbol _symbol, CSharpCompilation _compilation);
    }

}
