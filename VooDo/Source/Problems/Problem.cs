
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

        internal Problem(EKind _kind, ESeverity _severity, string _description, Node? _source)
            : this(_kind, _severity, _description, _source?.Origin, _source) { }

        internal Problem(EKind _kind, ESeverity _severity, string _description, Origin? _origin = null, Node? _source = null)
        {
            Kind = _kind;
            Severity = _severity;
            Description = _description;
            Origin = _origin;
            Source = _source;
        }

        public ESeverity Severity { get; }
        public EKind Kind { get; }
        public string Description { get; }
        public Node? Source { get; }
        public Origin? Origin { get; private set; }

        internal void AttachOrigin(Origin _origin)
        {
            if (Origin is null || Origin == Origin.Unknown)
            {
                Origin = _origin;
            }
        }

        public sealed override string ToString() => $"{GetType().Name}: {GetDisplayMessage()}";

        private string GetSourceMessage()
        {
            string code = Source!.ToString();
            object snippet = code.Length > 20 ? $"{code.Substring(0, 10)}...{code.Substring(code.Length - 10, 10)}" : code;
            string type = Source.GetType().Name;
            return $"{type} «{snippet}»";
        }

        public string GetDisplayMessage()
            => $"{Kind} {Severity}"
            + (Source is null ? "" : $"in {GetSourceMessage()}")
            + ((Origin ?? Source?.Origin) is Origin o ? $" @{o.GetDisplayMessage()}" : "")
            + $" {Description}";

    }

}
