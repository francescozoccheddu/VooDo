
using VooDo.AST;

namespace VooDo.Errors.Problems
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

        internal Problem(EKind _kind, ESeverity _severity, NodeOrIdentifier? _source, string _description)
        {
            Kind = _kind;
            Source = _source;
            Severity = _severity;
            Description = _description;
        }

        public NodeOrIdentifier? Source { get; }
        public EKind Kind { get; }
        public ESeverity Severity { get; }
        public string Description { get; }

        public string GetDisplayMessage()
            => $"{Severity}: {Description}" + (Source is not null ? $" ({Source.Origin.GetDisplayMessage()})" : "");

    }

}
