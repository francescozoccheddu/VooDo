
using Microsoft.CodeAnalysis;

using VooDo.AST;
using VooDo.Compilation.Emission;

namespace VooDo.Errors.Problems
{

    public class RoslynProblem : Problem
    {

        private RoslynProblem(EKind _kind, ESeverity _severity, NodeOrIdentifier? _source, string _description)
            : base(_kind, _severity, _source, _description)
        {
        }

        internal static RoslynProblem? FromDiagnostic(Diagnostic _diagnostic, Marker _marker, EKind _kind = EKind.Semantic)
        {
            if (_diagnostic.Severity is DiagnosticSeverity.Info or DiagnosticSeverity.Hidden)
            {
                return null;
            }
            NodeOrIdentifier? syntax = null;
            {
                SyntaxNode? root = _diagnostic.Location.SourceTree?.GetRoot();
                if (root is not null)
                {
                    SyntaxNode? node = root.FindNode(_diagnostic.Location.SourceSpan);
                    syntax = _marker.GetOwner(node);
                }
            }
            ESeverity severity = _diagnostic.Severity == DiagnosticSeverity.Error ? ESeverity.Error : ESeverity.Warning;
            return new RoslynProblem(_kind, severity, syntax, _diagnostic.GetMessage());
        }


    }

}
