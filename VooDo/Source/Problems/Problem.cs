
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

        public sealed override string ToString() => $"{GetType().Name}: {GetDisplayMessage()}";

    }

    public class SourceProblem : Problem
    {

        internal SourceProblem(EKind _kind, ESeverity _severity, string _description, Node? _source)
            : base(_kind, _severity, _description)
        {
            Source = _source;
        }

        public Node? Source { get; private set; }

        internal void AttachOrigin(Origin _origin)
        {
            if (Source is not null)
            {
                Source = Source with
                {
                    Origin = _origin
                };
            }
        }

        private string GetSourceMessage()
        {
            string code = Source!.ToString();
            object snippet = code.Length > 20 ? $"{code.Substring(0, 10)}...{code.Substring(code.Length - 10, 10)}" : code;
            string origin = Source.Origin.GetDisplayMessage();
            string type = Source.GetType().Name;
            return $"{type} «{snippet}» @ {origin}";
        }

        public override string GetDisplayMessage()
            => Source is null ? base.GetDisplayMessage()
            : $"{Kind} {Severity} in {GetSourceMessage()}: {Description}";

    }

}
