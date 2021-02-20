
using System;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST.Names;
using VooDo.Hooks;
using VooDo.Problems;
using VooDo.Utils;

namespace VooDo.Compiling
{

    public sealed record Options
    {

        public static Options Default { get; } = new Options();

        private Options()
        {
            References = Reference.GetSystemReferences().Add(Reference.RuntimeReference);
            Namespace = "VooDo.Generated";
            ClassName = "GeneratedProgram";
            ReturnType = null;
            AssemblyName = null;
            HookInitializer = NotifyPropertyChangedHookInitializer.Instance;
        }

        public string? AssemblyName { get; init; }

        public Namespace Namespace { get; init; }

        public Identifier ClassName { get; init; }

        public IHookInitializer HookInitializer { get; init; }

        public ComplexType? ReturnType { get; init; }

        private ImmutableArray<Reference> m_references;
        public ImmutableArray<Reference> References
        {
            get => m_references;
            set
            {
                m_references = Reference.Merge(value.EmptyIfDefault());
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
