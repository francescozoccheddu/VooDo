
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;

using VooDo.AST;
using VooDo.AST.Directives;
using VooDo.AST.Names;
using VooDo.AST.Statements;
using VooDo.Compiling;
using VooDo.Compiling.Emission;
using VooDo.Hooks;
using VooDo.Parsing;
using VooDo.Problems;
using VooDo.Utils;
using VooDo.WinUI.HookInitializers;

using Compilation = VooDo.Compiling.Compilation;

namespace VooDo.WinUI.Generator
{
    [Generator]
    internal sealed class ClassScriptGenerator : ISourceGenerator
    {


        private static string GetName(string _path)
        {
            string name = Path.GetFileNameWithoutExtension(_path);
            StringBuilder builder = new(Identifiers.ClassScripts.scriptPrefix.Length + name.Length);
            builder.Append(Identifiers.ClassScripts.scriptPrefix);
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
            return builder.ToString();
        }

        private static bool TryGetXamlName(AdditionalText _text, GeneratorExecutionContext _context, out Namespace? _namespace, out Identifier? _name)
        {
            _namespace = null;
            _name = null;
            AnalyzerConfigOptions? options = _context.AnalyzerConfigOptions.GetOptions(_text);
            string? xamlClassOption = OptionRetriever.Get(Identifiers.ClassScripts.xamlClassOption, _context, _text);
            string? xamlPathOption = OptionRetriever.Get(Identifiers.ClassScripts.xamlPathOption, _context, _text);
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
                    SyntaxTree tree = _context.Compilation.SyntaxTrees.SingleWithFile(xamlCbFile, _t => _t!.FilePath) ?? throw new Exception();
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
                catch (VooDoException) { }
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

        private static void Process(AdditionalText _text, GeneratorExecutionContext _context, ImmutableArray<UsingDirective> _usings, NameDictionary _nameDictionary, IHookInitializer _hookInitializer)
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
                Script script = Parser.Script(code, _text.Path);
                bool failed = false;
                foreach (ReturnStatement returnStatement in script.DescendantNodesAndSelf().OfType<ReturnStatement>())
                {
                    _context.ReportDiagnostic(DiagnosticFactory.ReturnNotAllowed((CodeOrigin)returnStatement.Origin));
                    failed = true;
                }
                if (failed)
                {
                    return;
                }
                script = script.AddUsingDirectives(_usings);
                script = script.AddGlobals(new Global(true, new QualifiedType(xamlNamespace, xamlName!), Identifiers.ClassScripts.thisVariableName));
                ProgramTag pathTag = new(Identifiers.ClassScripts.pathTag, FilePaths.Normalize(_text.Path));
                ProgramTag tagTag = new(Identifiers.ClassScripts.tagTag, OptionRetriever.Get(Identifiers.ClassScripts.tagOption, _context, _text));
                Options options = Options.Default with
                {
                    Namespace = xamlNamespace,
                    ClassName = name,
                    References = default,
                    HookInitializer = _hookInitializer,
                    ContainingClass = xamlName,
                    Accessibility = Options.EAccessibility.Private,
                    Tags = ImmutableArray.Create(pathTag, tagTag)
                };
                Compilation compilation = Compilation.SucceedOrThrow(script, options, _context.CancellationToken, (CSharpCompilation)_context.Compilation);
                _context.AddSource(name, compilation.GetCSharpSourceCode());
            }
            catch (VooDoException exception)
            {
                foreach (Problem problem in exception.Problems)
                {
                    Origin origin = problem.Origin ?? Origin.Unknown;
                    _context.ReportDiagnostic(DiagnosticFactory.CompilationError(problem.Description, origin as CodeOrigin, problem.Severity));
                }
            }
        }

        public void Execute(GeneratorExecutionContext _context)
        {
            if (!UsingsOption.TryGet(_context, out ImmutableArray<UsingDirective> usingDirectives))
            {
                return;
            }
            NameDictionary nameDictionary = new();
            IHookInitializer hookInitializer = new HookInitializerList(new DependencyPropertyHookInitializer(), new NotifyPropertyChangedHookInitializer());
            foreach (AdditionalText text in _context.AdditionalFiles.Where(_f => Path.GetExtension(_f.Path).Equals(Identifiers.scriptFileExtension, FilePaths.SystemComparison)))
            {
                if (_context.CancellationToken.IsCancellationRequested)
                {
                    _context.ReportDiagnostic(DiagnosticFactory.Canceled());
                    return;
                }
                Process(text, _context, usingDirectives, nameDictionary, hookInitializer);
            }
        }

        public void Initialize(GeneratorInitializationContext _context)
        {
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                //System.Diagnostics.Debugger.Launch();
            }
        }

    }

}
