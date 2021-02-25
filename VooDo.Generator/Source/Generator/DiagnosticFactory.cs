﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

using VooDo.AST;
using VooDo.Problems;

namespace VooDo.Generator
{

    internal static class DiagnosticFactory
    {

        private static int s_idCount = 0;
        private static string s_Id => $"VD{++s_idCount:00}";

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
                s_Id,
                "File read error",
                "Error while reading script file '{0}'",
                "Generator",
                DiagnosticSeverity.Error,
                true,
                "Make sure that the file can be read and that is not in use by other applications.");

        private static readonly DiagnosticDescriptor s_returnNotAllowedDescriptor =
            new(
                s_Id,
                "Return not allowed",
                "Standalone script cannot return values",
                "Generator",
                DiagnosticSeverity.Error,
                true,
                "Standalone scripts cannot return values. Use markup extensions instead.");

        private static readonly DiagnosticDescriptor s_compilationErrorDescriptor =
            new(
                s_Id,
                "Compilation error",
                "{0}",
                "Generator",
                DiagnosticSeverity.Error,
                true);

        private static readonly DiagnosticDescriptor s_compilationWarningDescriptor =
            new(
                s_Id,
                "Compilation warning",
                "{0}",
                "Generator",
                DiagnosticSeverity.Warning,
                true);

        private static readonly DiagnosticDescriptor s_invalidXamlPath =
            new(
                s_Id,
                "Invalid XAML path",
                "XAML owner '{1}' for script '{0}' does not exist or is not a valid XAML class",
                "Generator",
                DiagnosticSeverity.Error,
                true,
                "Make sure that the file contains a valid XAML class and is correctly added to the project.");

        private static readonly DiagnosticDescriptor s_invalidXamlClass =
            new(
                s_Id,
                "Invalid XAML class",
                "XAML owner '{1}' for script '{0}' is not a valid class name",
                "Generator",
                DiagnosticSeverity.Error,
                true,
                "Make sure that the file contains a valid XAML class and is correctly added to the project.");

        private static readonly DiagnosticDescriptor s_cannotInferXaml =
            new(
                s_Id,
                "Cannot infer XAML class",
                "XAML owner for script '{0}' cannot be inferred",
                "Generator",
                DiagnosticSeverity.Error,
                true,
                "Make sure that the file has the same name and is in the same directory of the XAML owner or manually specify either XamlClass or XamlPath.");


        private static readonly DiagnosticDescriptor s_invalidUsingDescriptor =
            new(
                s_Id,
                "Invalid using directive",
                "Invalid global using directive '{0}': {1}",
                "Generator",
                DiagnosticSeverity.Error,
                true,
                "The global using directive option must specify a list of semicolon separated namespaces with optional aliases in the form 'alias=namespace'.");

        private static readonly DiagnosticDescriptor s_bothXamlClassAndXamlPathDescriptor =
            new(
                s_Id,
                "Both XamlClass and XamlPath specified",
                "File '{0}' cannot specify both XamlClass and XamlPath",
                "Generator",
                DiagnosticSeverity.Error,
                true);

        private static readonly DiagnosticDescriptor s_canceledDescriptor =
            new(
                s_Id,
                "Generation canceled",
                "Generation canceled",
                "Generator",
                DiagnosticSeverity.Warning,
                true);

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

        internal static Diagnostic InvalidXamlPath(string _scriptFile, string _xamlPath)
            => Diagnostic.Create(
                s_invalidXamlPath,
                null,
                _scriptFile,
                _xamlPath);

        internal static Diagnostic InvalidXamlClass(string _scriptFile, string _xamlClass)
            => Diagnostic.Create(
                s_invalidXamlClass,
                null,
                _scriptFile,
                _xamlClass);

        internal static Diagnostic CannotInferXaml(string _scriptFile)
            => Diagnostic.Create(
                s_cannotInferXaml,
                null,
                _scriptFile);

        internal static Diagnostic InvalidUsing(string _directive, string _message)
            => Diagnostic.Create(
                s_invalidUsingDescriptor,
                null,
                _directive,
                _message);

        internal static Diagnostic BothXamlOptions(string _file)
            => Diagnostic.Create(
                s_bothXamlClassAndXamlPathDescriptor,
                null,
                _file);

        internal static Diagnostic Canceled()
            => Diagnostic.Create(
                s_canceledDescriptor,
                null);

    }

}
