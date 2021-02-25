using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

using VooDo.AST;
using VooDo.Problems;

namespace VooDo.Generator
{

    internal static class DiagnosticFactory
    {

        private static Location GetLocation(CodeOrigin? _origin, string _file)
        {
            if (_origin is not null)
            {
                TextSpan span = new(_origin.Start, _origin.Length);
                _origin.GetLinePosition(out int startLine, out int startCharacter, out int endLine, out int endCharacter);
                LinePositionSpan lineSpan = new(new LinePosition(startLine, startCharacter), new LinePosition(endLine, endCharacter));
                return Location.Create(_file, span, lineSpan);
            }
            else
            {
                return Location.Create(_file, default, default);
            }
        }

        private static readonly DiagnosticDescriptor s_fileReadErrorDescriptor =
            new(
                "VD001",
                "File read error",
                "Error while reading script file '{0}'",
                "Generator",
                DiagnosticSeverity.Error,
                true,
                "Make sure that the file can be read and that is not in use by other applications.");

        private static readonly DiagnosticDescriptor s_returnNotAllowedDescriptor =
            new(
                "VD003",
                "Return not allowed",
                "Standalone script cannot return values",
                "Generator",
                DiagnosticSeverity.Error,
                true,
                "Standalone scripts cannot return values. Use markup extensions instead.");

        private static readonly DiagnosticDescriptor s_compilationErrorDescriptor =
            new(
                "VD004",
                "Compilation error",
                "{0}",
                "Generator",
                DiagnosticSeverity.Error,
                true);

        private static readonly DiagnosticDescriptor s_compilationWarningDescriptor =
            new(
                "VD002",
                "Compilation warning",
                "{0}",
                "Generator",
                DiagnosticSeverity.Warning,
                true);

        private static readonly DiagnosticDescriptor s_xamlNotFoundDescriptor =
            new(
                "VD005",
                "XAML not found",
                "XAML owner '{1}' for script '{0}' does not exist or is not a valid XAML class",
                "Generator",
                DiagnosticSeverity.Error,
                true,
                "Make sure that the file contains a valid XAML class and is correctly added to the project.");

        private static readonly DiagnosticDescriptor s_invalidUsingDescriptor =
            new(
                "VD006",
                "Invalid using directive",
                "Invalid global using directive '{0}': {1}",
                "Generator",
                DiagnosticSeverity.Error,
                true,
                "The global using directive option must specify a list of semicolon separated namespaces with optional aliases in the form 'alias=namespace'.");

        internal static Diagnostic FileReadError(string _file)
            => Diagnostic.Create(
                s_fileReadErrorDescriptor,
                null,
                _file);

        internal static Diagnostic ReturnNotAllowed(CodeOrigin _origin, string _file)
            => Diagnostic.Create(
                s_returnNotAllowedDescriptor,
                GetLocation(_origin, _file));

        internal static Diagnostic CompilationError(string _message, Origin _origin, string _file, Problem.ESeverity _severity)
            => Diagnostic.Create(
                _severity switch
                {
                    Problem.ESeverity.Error => s_compilationErrorDescriptor,
                    Problem.ESeverity.Warning => s_compilationWarningDescriptor
                },
                GetLocation(_origin as CodeOrigin, _file),
                _message);

        internal static Diagnostic XamlNotFound(string _scriptFile, string _xamlFile)
            => Diagnostic.Create(
                s_xamlNotFoundDescriptor,
                null,
                _scriptFile,
                _xamlFile);

        internal static Diagnostic InvalidUsing(string _directive, string _message)
            => Diagnostic.Create(
                s_invalidUsingDescriptor,
                null,
                _directive,
                _message);

    }

}
