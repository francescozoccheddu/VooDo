
using VooDo.AST;

namespace VooDo.Problems
{

    public class Problem
    {

        public enum ESeverity
        {
            Error, Warning
        }

        public enum EKind
        {
            Syntactic, Semantic, Emission
        }

        internal Problem(EKind _kind, ESeverity _severity, string _description)
        {
            Kind = _kind;
            Severity = _severity;
            Description = _description;
        }

        public ESeverity Severity { get; }
        public EKind Kind { get; }
        public string Description { get; }

        public virtual string GetDisplayMessage()
            => $"{Kind} {Severity}: {Description}";

    }

    public class SourceProblem : Problem
    {

        internal SourceProblem(EKind _kind, ESeverity _severity, string _description, Node? _source)
            : base(_kind, _severity, _description)
        {
            Source = _source;
        }

        public Node? Source { get; }

        private string GetSourceMessage()
        {
            string code = Source!.ToString();
            object snippet = code.Length > 20 ? $"{code[0..10]}…{code[^1..^10]}" : code;
            string origin = Source.Origin.GetDisplayMessage();
            string type = Source.GetType().Name;
            return $"{type} ❮{snippet}❯ @ {origin}";
        }

        public override string GetDisplayMessage()
            => Source is null ? base.GetDisplayMessage()
            : $"{Kind} {Severity} in {Source.GetType().Name} {GetSourceMessage()} : {Description}";

    }

}
