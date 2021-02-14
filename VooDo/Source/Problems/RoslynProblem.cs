
using Microsoft.CodeAnalysis;

using VooDo.AST;
using VooDo.Compiling.Emission;

namespace VooDo.Problems
{

    public class RoslynProblem : SourceProblem
    {

        private RoslynProblem(EKind _kind, ESeverity _severity, Node? _source, string _description)
            : base(_kind, _severity, _description, _source)
        {
        }

        internal static RoslynProblem? FromDiagnostic(Diagnostic _diagnostic, Tagger _tagger, EKind _kind = EKind.Semantic)
        {
            if (_diagnostic.Severity is DiagnosticSeverity.Info or DiagnosticSeverity.Hidden)
            {
                return null;
            }
            Node? syntax = null;
            {
                SyntaxNode? root = _diagnostic.Location.SourceTree?.GetRoot();
                if (root is not null)
                {
                    SyntaxNode? node = root.FindNode(_diagnostic.Location.SourceSpan);
                    syntax = _tagger.GetOwner(node);
                }
            }
            ESeverity severity = _diagnostic.Severity == DiagnosticSeverity.Error ? ESeverity.Error : ESeverity.Warning;
            return new RoslynProblem(_kind, severity, syntax, _diagnostic.GetMessage());
        }


    }

}
