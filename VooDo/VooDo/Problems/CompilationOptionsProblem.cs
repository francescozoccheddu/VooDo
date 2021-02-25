
using VooDo.Compiling;

namespace VooDo.Problems
{

    public class CompilationOptionsProblem : Problem
    {

        public Options Options { get; }

        internal CompilationOptionsProblem(string _description, Options _options)
            : base(EKind.Semantic, ESeverity.Error, _description)
        {
            Options = _options;
        }

    }

    public sealed class CompilationOptionsPropertyProblem : CompilationOptionsProblem
    {

        public string Property { get; }

        internal CompilationOptionsPropertyProblem(string _description, Options _options, string _property)
            : base(_description, _options)
        {
            Property = _property;
        }

    }

}
