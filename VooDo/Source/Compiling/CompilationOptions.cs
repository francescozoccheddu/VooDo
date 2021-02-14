
using System;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST.Names;
using VooDo.Problems;
using VooDo.Utils;

namespace VooDo.Compiling
{

    public sealed record CompilationOptions
    {

        public static CompilationOptions Default { get; } = new CompilationOptions(null, Reference.GetSystemReferences().Add(Reference.RuntimeReference));

        public CompilationOptions(ComplexType? _returnType = null, ImmutableArray<Reference> _references = default)
        {
            ReturnType = _returnType;
            References = _references;
        }

        public ComplexType? ReturnType { get; init; }

        private ImmutableArray<Reference> m_references;
        public ImmutableArray<Reference> References
        {
            get => m_references;
            set
            {
                if (value.IsDefault)
                {
                    m_references = Default.References;
                }
                else
                {
                    m_references = Reference.Merge(value);
                    int runtimeIndex = m_references.IndexOf(Reference.RuntimeReference, Reference.MetadataEqualityComparer);
                    if (runtimeIndex < 0)
                    {
                        throw new CompilationOptionsPropertyProblem("No runtime reference", this, nameof(References)).AsThrowable();
                    }
                    if (!m_references[runtimeIndex].Aliases.Contains(CompilationConstants.runtimeReferenceAlias))
                    {
                        throw new CompilationOptionsPropertyProblem($"Runtime reference must define '{CompilationConstants.runtimeReferenceAlias}' alias", this, nameof(References)).AsThrowable();
                    }
                    if (m_references.SelectMany(_r => _r.Aliases).FirstDuplicate(out Identifier? duplicateAlias))
                    {
                        throw new CompilationOptionsPropertyProblem($"Duplicate reference alias '{duplicateAlias}'", this, nameof(References)).AsThrowable();
                    }
                }
            }
        }

    }

}
