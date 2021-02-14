
using VooDo.Compiling;

namespace VooDo.Problems
{

    public class CompilationOptionsProblem : Problem
    {

        public CompilationOptions Options { get; }

        internal CompilationOptionsProblem(string _description, CompilationOptions _options)
            : base(EKind.Semantic, ESeverity.Error, _description)
        {
            Options = _options;
        }

    }

    public sealed class CompilationOptionsPropertyProblem : CompilationOptionsProblem
    {

        public string Property { get; }

        internal CompilationOptionsPropertyProblem(string _description, CompilationOptions _options, string _property)
            : base(_description, _options)
        {
            Property = _property;
        }

    }

}
