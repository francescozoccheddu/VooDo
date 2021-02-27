
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST.Expressions;
using VooDo.AST.Names;
using VooDo.Hooks;
using VooDo.Utils;
using VooDo.WinUI.Generator;

namespace VooDo.WinUI.HookInitializers
{

    internal abstract class HookInitializer : IHookInitializer
    {

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
            m_type = new(Namespace.Parse(Identifiers.hooksNamespace), new Identifier(HookTypeName));
        }

        protected abstract Identifier HookTypeName { get; }

        protected Expression GetInitializer(CSharpCompilation _compilation, params Argument[] _arguments)
        {
            if (m_canCache && m_cachedCompilation != _compilation)
            {
                IEnumerable<MetadataReference>? references = ReferenceFinder.OrderByFileNameHint(_compilation.References, Identifiers.vooDoWinUiName);
                MetadataReference? reference = ReferenceFinder.FindByNamespace(Identifiers.hooksNamespace, _compilation, references).FirstOrDefault();
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
