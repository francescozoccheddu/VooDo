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
            References = ImmutableArray.Create<Reference>();
            Namespace = "VooDo.Generated";
            ClassName = "GeneratedProgram";
            ReturnType = null;
            AssemblyName = null;
            Accessibility = EAccessibility.Public;
            HookInitializer = new HookInitializerList();
        }

        public string? AssemblyName { get; init; }

        public Namespace? Namespace { get; init; }

        public Identifier ClassName { get; init; }

        public Identifier? ContainingClass { get; init; }

        public enum EAccessibility
        {
            Public, Protected, Private, Internal, PrivateProtected, InternalProtected
        }

        public EAccessibility Accessibility { get; init; }

        public IHookInitializer HookInitializer { get; init; }

        public ComplexType? ReturnType { get; init; }

        private ImmutableArray<Reference> m_references;
        public ImmutableArray<Reference> References
        {
            get => m_references;
            set
            {
                m_references = Reference.Merge(value.EmptyIfDefault());
                if (m_references.SelectMany(_r => _r.Aliases).FirstDuplicate(out Identifier? duplicateAlias))
                {
                    throw new CompilationOptionsPropertyProblem($"Duplicate reference alias '{duplicateAlias}'", this, nameof(References)).AsThrowable();
                }
            }
        }

    }

}
