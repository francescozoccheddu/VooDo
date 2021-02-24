
using Microsoft.CodeAnalysis;
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

using VC = VooDo.Compiling;

namespace VooDo.Generator
{
    [Generator]
    internal sealed class Generator : ISourceGenerator
    {

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

        private static void Process(AdditionalText _text, GeneratorExecutionContext _context, ImmutableArray<VC::Reference> _references, ImmutableArray<UsingDirective> _usings, NameDictionary _nameDictionary)
        {
            SourceText? sourceText = _text.GetText(_context.CancellationToken);
            if (sourceText is null)
            {
                _context.ReportDiagnostic(DiagnosticFactory.FileReadError(_text.Path));
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
                    Namespace = "VooDo.Generated",
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

        public void Execute(GeneratorExecutionContext _context)
        {
            NameDictionary nameDictionary = new();
            ImmutableArray<UsingDirective> usingDirectives = ImmutableArray.Create<UsingDirective>();
            ImmutableArray<VC::Reference> references = _context.Compilation.References
                .OfType<PortableExecutableReference>()
                .Where(_r => _r.FilePath is not null && !Path.GetFileName(_r.FilePath).Equals("VooDo.Runtime.dll", StringComparison.OrdinalIgnoreCase))
                .Select(_r => VC::Reference.FromFile(_r.FilePath!, _r.Properties.Aliases.Select(_a => new Identifier(_a))))
                .Concat(new[] { VC::Reference.RuntimeReference })
                .ToImmutableArray();
            foreach (AdditionalText text in _context.AdditionalFiles.Where(_f => Path.GetExtension(_f.Path).Equals(".voodo", StringComparison.OrdinalIgnoreCase)))
            {
                Process(text, _context, references, usingDirectives, nameDictionary);
            }
        }

        public void Initialize(GeneratorInitializationContext _context)
        {
            return;
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Launch();
            }
        }

    }

}
