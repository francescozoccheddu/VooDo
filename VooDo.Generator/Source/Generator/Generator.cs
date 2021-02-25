
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;

using VooDo.AST;
using VooDo.AST.Directives;
using VooDo.AST.Names;
using VooDo.AST.Statements;
using VooDo.Parsing;
using VooDo.Problems;
using VooDo.Utils;

using VC = VooDo.Compiling;

namespace VooDo.Generator
{
    [Generator]
    internal sealed class Generator : ISourceGenerator
    {

        private const string c_projectOptionPrefix = "build_property";
        private const string c_fileOptionPrefix = "build_metadata.AdditionalFiles";
        private const string c_usingsOption = c_projectOptionPrefix + ".VooDoUsings";
        private const string c_xamlPathOption = c_fileOptionPrefix + ".XamlPath";
        private const string c_xamlClassOption = c_fileOptionPrefix + ".XamlClass";

        private sealed class NameDictionary
        {

            private readonly Dictionary<string, int> m_names = new();

            internal string TakeName(string _name)
            {
                int count = m_names.TryGetValue(_name, out int value) ? value : 0;
                m_names[_name] = count++;
                if (count > 1)
                {
                    _name = $"{_name}{count}";
                }
                return _name;
            }

        }

        private static string GetName(string _path)
        {
            string name = Path.GetFileNameWithoutExtension(_path);
            StringBuilder builder = new(name.Length);
            bool initial = true;
            foreach (char c in name)
            {
                if (c is (>= '0' and <= '9') or (>= 'A' and <= 'Z') or (>= 'a' and <= 'z') or '_')
                {
                    builder.Append(initial ? char.ToUpper(c) : c);
                    initial = false;
                }
                else
                {
                    initial = true;
                }
            }
            if (builder.Length == 0 || char.IsDigit(builder[0]))
            {
                builder.Insert(0, "Script");
            }
            return builder.ToString();
        }

        private static bool TryGetXamlName(AdditionalText _text, GeneratorExecutionContext _context, out Namespace? _namespace, out Identifier? _name)
        {
            _namespace = null;
            _name = null;
            AnalyzerConfigOptions? options = _context.AnalyzerConfigOptions.GetOptions(_text);
            options.TryGetValue(c_xamlClassOption, out string? xamlClassOption);
            options.TryGetValue(c_xamlPathOption, out string? xamlPathOption);
            if (string.IsNullOrEmpty(xamlClassOption))
            {
                xamlClassOption = null;
            }
            if (string.IsNullOrEmpty(xamlPathOption))
            {
                xamlPathOption = null;
            }
            if (xamlClassOption is not null && xamlPathOption is not null)
            {
                _context.ReportDiagnostic(DiagnosticFactory.BothXamlOptions(_text.Path));
            }
            if (xamlClassOption is null)
            {
                try
                {
                    string xamlCbFile;
                    if (xamlPathOption is null)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(_text.Path);
                        string directory = Path.GetDirectoryName(_text.Path);
                        xamlCbFile = Path.Combine(directory, $"{fileName}.xaml.cs");
                    }
                    else
                    {
                        string fileDirectory = Path.GetDirectoryName(_text.Path);
                        string path = Path.IsPathRooted(xamlPathOption)
                            ? xamlPathOption
                            : Path.Combine(fileDirectory, xamlPathOption);
                        xamlCbFile = Path.GetExtension(path) switch
                        {
                            ".xaml" => $"{path}.cs",
                            ".cs" => path,
                            _ => $"{path}.xaml.cs",
                        };
                    }
                    xamlCbFile = NormalizeFilePath.Normalize(xamlCbFile);
                    SyntaxTree tree = _context.Compilation.SyntaxTrees.Single(_t => NormalizeFilePath.Normalize(_t.FilePath).Equals(xamlCbFile));
                    ImmutableArray<ClassDeclarationSyntax> classes = tree.GetRoot(_context.CancellationToken)
                        .DescendantNodesAndSelf()
                        .OfType<ClassDeclarationSyntax>()
                        .Where(_c => _c.Modifiers.Any(_m => _m.IsKind(SyntaxKind.PartialKeyword)))
                        .ToImmutableArray();
                    if (classes.Length > 1)
                    {
                        string name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(tree.FilePath));
                        classes = classes.Where(_c => _c.Identifier.ValueText == name).ToImmutableArray();
                    }
                    if (classes.IsEmpty)
                    {
                        throw new Exception("No candidate class");
                    }
                    SemanticModel semantics = _context.Compilation.GetSemanticModel(tree);
                    INamedTypeSymbol? symbol = semantics.GetDeclaredSymbol(classes[0]);
                    if (symbol is null)
                    {
                        throw new Exception("Unknown class symbol");
                    }
                    string? namespaceName = symbol.ContainingNamespace?.ToDisplayString();
                    _namespace = namespaceName is null ? null : Namespace.Parse(namespaceName);
                    _name = symbol.Name;
                }
                catch
                {
                    Diagnostic diagnostic = xamlPathOption is null
                        ? DiagnosticFactory.CannotInferXaml(_text.Path)
                        : DiagnosticFactory.InvalidXamlPath(_text.Path, xamlPathOption);
                    _context.ReportDiagnostic(diagnostic);
                    return false;
                }
            }
            else
            {
                QualifiedType? type = null;
                try
                {
                    type = QualifiedType.Parse(xamlClassOption);
                }
                catch { }
                if (type is null || type.IsArray || type.IsAliasQualified || type.IsNullable || type.Path.Any(_t => _t.IsGeneric))
                {
                    _context.ReportDiagnostic(DiagnosticFactory.InvalidXamlClass(_text.Path, xamlClassOption));
                    return false;
                }
                _namespace = type.IsNamespaceQualified ? new Namespace(type.Path.Take(type.Path.Length - 1).Select(_t => _t.Name)) : null;
                _name = type.Path.Last().Name;
            }
            return true;
        }

        private static void Process(AdditionalText _text, GeneratorExecutionContext _context, ImmutableArray<VC::Reference> _references, ImmutableArray<UsingDirective> _usings, NameDictionary _nameDictionary)
        {
            SourceText? sourceText = _text.GetText(_context.CancellationToken);
            if (sourceText is null)
            {
                _context.ReportDiagnostic(DiagnosticFactory.FileReadError(_text.Path));
                return;
            }
            if (!TryGetXamlName(_text, _context, out Namespace? xamlNamespace, out Identifier? xamlName))
            {
                return;
            }
            string name = _nameDictionary.TakeName(GetName(_text.Path));
            string code = sourceText.ToString();
            try
            {
                Script script = Parser.Script(code);
                bool failed = false;
                foreach (ReturnStatement returnStatement in script.DescendantNodesAndSelf().OfType<ReturnStatement>())
                {
                    _context.ReportDiagnostic(DiagnosticFactory.ReturnNotAllowed((CodeOrigin)returnStatement.Origin, _text.Path));
                    failed = true;
                }
                if (failed)
                {
                    return;
                }
                script = script.AddUsingDirectives(_usings);
                VC::Options options = VC::Options.Default with
                {
                    References = _references,
                    Namespace = xamlNamespace,
                    ClassName = name
                };
                VC::Compilation compilation = VC::Compilation.SucceedOrThrow(script, options);
                _context.AddSource(name, compilation.GetCSharpSourceCode());
            }
            catch (VooDoException exception)
            {
                foreach (Problem problem in exception.Problems)
                {
                    Origin origin = problem.Origin ?? Origin.Unknown;
                    _context.ReportDiagnostic(DiagnosticFactory.CompilationError(problem.Description, origin, _text.Path, problem.Severity));
                }
            }
        }

        private static UsingDirective ParseUsing(string _value)
        {
            string[] tokens = _value.Split('=');
            if (tokens.Length == 1)
            {
                string[] nameTokens = tokens[0].Split();
                if (nameTokens.Length > 2 || (nameTokens.Length == 2 && !nameTokens[0].Equals("static", StringComparison.OrdinalIgnoreCase)))
                {
                    throw new FormatException("Name cannot contain whitespace");
                }
                if (nameTokens.Length == 1)
                {
                    return new UsingNamespaceDirective(nameTokens[0]);
                }
                else
                {
                    return new UsingStaticDirective(nameTokens[1]);
                }
            }
            else if (tokens.Length == 2)
            {
                return new UsingNamespaceDirective(tokens[0], tokens[1]);
            }
            else
            {
                throw new FormatException("Multiple '=' symbols");
            }
        }

        private static bool TryGetUsings(GeneratorExecutionContext _context, out ImmutableArray<UsingDirective> _directives)
        {
            if (_context.AnalyzerConfigOptions.GlobalOptions.TryGetValue(c_usingsOption, out string? option))
            {
                string[] tokens = option.Split(',');
                int count = string.IsNullOrEmpty(tokens.Last()) ? tokens.Length - 1 : tokens.Length;
                UsingDirective[] directives = new UsingDirective[count];
                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        directives[i] = ParseUsing(tokens[i]);
                    }
                    catch (Exception e)
                    {
                        _context.ReportDiagnostic(DiagnosticFactory.InvalidUsing(tokens[i], e.Message));
                        return false;
                    }
                }
                _directives = directives.ToImmutableArray();
            }
            else
            {
                _directives = ImmutableArray.Create<UsingDirective>();
            }
            return true;
        }

        public void Execute(GeneratorExecutionContext _context)
        {
            if (!TryGetUsings(_context, out ImmutableArray<UsingDirective> usingDirectives))
            {
                return;
            }
            NameDictionary nameDictionary = new();
            ImmutableArray<VC::Reference> references = _context.Compilation.References
                .OfType<PortableExecutableReference>()
                .Where(_r => _r.FilePath is not null && !Path.GetFileName(_r.FilePath).Equals("VooDo.Runtime.dll", StringComparison.OrdinalIgnoreCase))
                .Select(_r => VC::Reference.FromFile(_r.FilePath!, _r.Properties.Aliases.Select(_a => new Identifier(_a))))
                .Concat(new[] { VC::Reference.RuntimeReference })
                .ToImmutableArray();
            foreach (AdditionalText text in _context.AdditionalFiles.Where(_f => Path.GetExtension(_f.Path).Equals(".voodo", StringComparison.OrdinalIgnoreCase)))
            {
                if (_context.CancellationToken.IsCancellationRequested)
                {
                    _context.ReportDiagnostic(DiagnosticFactory.Canceled());
                    return;
                }
                Process(text, _context, references, usingDirectives, nameDictionary);
            }
        }

        public void Initialize(GeneratorInitializationContext _context)
        {
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Launch();
            }
        }

    }

}
