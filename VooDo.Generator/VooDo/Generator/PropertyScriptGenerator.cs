using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Portable.Xaml;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

using VooDo.AST;
using VooDo.AST.Directives;
using VooDo.AST.Names;
using VooDo.Hooks;
using VooDo.Parsing;
using VooDo.Problems;
using VooDo.Utils;
using VooDo.WinUI.HookInitializers;

using VC = VooDo.Compiling;

namespace VooDo.Generator
{

    [Generator]
    internal sealed class PropertyScriptGenerator : ISourceGenerator
    {

        private const string c_namePrefix = "VooDo_GeneratedPropertyScript_";

        private readonly struct Markup
        {

            public Markup(Token? _code, Token? _path, Token? _tag, Token _property, TypeToken _object, Token _markupObject)
            {
                Code = _code;
                Path = _path;
                Tag = _tag;
                Property = _property;
                Object = _object;
                MarkupObject = _markupObject;
            }

            public Token? Code { get; }
            public Token? Path { get; }
            public Token? Tag { get; }
            public Token Property { get; }
            public TypeToken Object { get; }
            public Token MarkupObject { get; }

        }

        private readonly struct Token
        {

            public Token(string _value, int _line, int _column)
            {
                Value = _value;
                Line = _line;
                Column = _column;
            }

            public string Value { get; }
            public int Line { get; }
            public int Column { get; }
            public int Length => Value.Length;

        }

        private readonly struct TypeToken
        {

            public TypeToken(string _name, string _namespace, int _line, int _column)
                : this(_name, _namespace, _line, _column, _name.Length) { }

            public TypeToken(string _name, string _namespace, int _line, int _column, int _length)
            {
                Name = _name;
                Namespace = _namespace;
                Line = _line;
                Column = _column;
                Length = _length;
            }

            public string Name { get; }
            public string Namespace { get; }
            public int Line { get; }
            public int Column { get; }
            public int Length { get; }
        }

        private static void Parse(string _xaml, out TypeToken _root, out ImmutableArray<Markup> _markups)
        {
            _root = default;
            XamlXmlReader? reader = new XamlXmlReader(new StringReader(_xaml));
            List<Markup> markups = new();
            Token targetProperty = default;
            TypeToken targetObject = default;
            Token? code = null, tag = null, path = null;
            XamlMember markupProperty = null!;
            Token? markupObject = null;
            bool isRootSet = false;
            bool isRootClass = false;
            bool isRootConfirmed = false;
            while (!reader.IsEof)
            {
                reader.Read();
                if (!isRootConfirmed)
                {
                    switch (reader.NodeType)
                    {
                        case XamlNodeType.StartObject when !isRootSet:
                            _root = new TypeToken(reader.Type.Name, reader.Type.PreferredXamlNamespace, reader.LineNumber - 1, reader.LinePosition - 1);
                            isRootSet = true;
                            break;
                        case XamlNodeType.StartObject:
                            isRootConfirmed = true;
                            break;
                        case XamlNodeType.StartMember when reader.Member == XamlLanguage.Class:
                            isRootClass = true;
                            break;
                        case XamlNodeType.Value when isRootClass:
                            string[] tokens = ((string)reader.Value).Split('.');
                            string name = tokens.Last();
                            string nameSpace = "using:" + string.Join(".", tokens.SkipLast());
                            _root = new TypeToken(name, nameSpace, reader.LineNumber - 1, reader.LinePosition - 1, 0);
                            isRootConfirmed = true;
                            break;
                    }
                }
                switch (reader.NodeType)
                {
                    case XamlNodeType.StartObject when reader.Type.Name == "VooDo" && reader.Type.PreferredXamlNamespace.StartsWith("using:VooDo.WinUI.Xaml"):
                        markupObject = new Token(reader.Type.Name, reader.LineNumber - 1, reader.LinePosition - 1);
                        code = tag = path = null;
                        break;
                    case XamlNodeType.StartObject:
                        targetObject = new TypeToken(reader.Type.Name, reader.Type.PreferredXamlNamespace, reader.LineNumber - 1, reader.LinePosition - 1);
                        break;
                    case XamlNodeType.EndObject when markupObject is not null:
                        markups.Add(new Markup(code, path, tag, targetProperty, targetObject, markupObject.Value));
                        markupObject = null;
                        break;
                    case XamlNodeType.StartMember when markupObject is not null:
                        markupProperty = reader.Member;
                        break;
                    case XamlNodeType.StartMember when reader.Member != XamlLanguage.UnknownContent:
                        targetProperty = new Token(reader.Member.Name, reader.LineNumber - 1, reader.LinePosition - 1);
                        break;
                    case XamlNodeType.Value when markupObject is not null && (markupProperty.Name == "Code" || markupProperty == XamlLanguage.Initialization):
                        code = new Token((string)reader.Value, reader.LineNumber - 1, reader.LinePosition - 1);
                        break;
                    case XamlNodeType.Value when markupObject is not null && markupProperty.Name == "Tag":
                        tag = new Token((string)reader.Value, reader.LineNumber - 1, reader.LinePosition - 1);
                        break;
                    case XamlNodeType.Value when markupObject is not null && markupProperty.Name == "Path":
                        path = new Token((string)reader.Value, reader.LineNumber - 1, reader.LinePosition - 1);
                        break;
                }
            }
            _markups = markups.ToImmutableArray();
        }

        public void Initialize(GeneratorInitializationContext _context) { }

        private static IEnumerable<string> GetXamlFilePaths(GeneratorExecutionContext _context)
        {
            foreach (SyntaxTree tree in _context.Compilation.SyntaxTrees)
            {
                string csPath = tree.FilePath;
                if (csPath.EndsWith(".xaml.cs"))
                {
                    yield return csPath.Substring(0, csPath.Length - 3);
                }
            }
        }

        private static int GetCharacterIndex(int _line, int _column, string _source)
        {
            int start = 0;
            while (_line > 0)
            {
                if (_source[start] == '\n')
                {
                    _line--;
                }
                start++;
            }
            return start + _column;
        }

        private static CodeOrigin GetOrigin(int _line, int _column, string _source, string _sourcePath, int _length = 0)
            => new CodeOrigin(GetCharacterIndex(_line, _column, _source), _length, _source, _sourcePath);

        private static CodeOrigin GetOrigin(Token _token, string _source, string _sourcePath)
            => new CodeOrigin(GetCharacterIndex(_token.Line, _token.Column, _source), _token.Length, _source, _sourcePath);

        private static CodeOrigin GetOrigin(TypeToken _token, string _source, string _sourcePath)
            => new CodeOrigin(GetCharacterIndex(_token.Line, _token.Column, _source), _token.Length, _source, _sourcePath);

        private static bool TryResolveWinUIXamlType(string _name, GeneratorExecutionContext _context, MetadataReference _winUi, CodeOrigin _origin, out QualifiedType? _type)
        {
            ImmutableArray<QualifiedType>? candidates = ReferenceFinder.FindTypeByPartialName(_name, _context.Compilation, _winUi)
                ?.Where(_c => _c.Path.Length > 3
                    && _c.Path[0].Name == "Microsoft"
                    && _c.Path[1].Name == "UI"
                    && _c.Path[2].Name == "Xaml")
                .ToImmutableArray();
            if (!candidates.HasValue)
            {
                _context.ReportDiagnostic(DiagnosticFactory.NoWinUI());
                _type = null;
                return false;
            }
            if (candidates?.Length == 1)
            {
                _type = candidates.Value[0] with { Origin = _origin };
                return true;
            }
            else
            {
                _context.ReportDiagnostic(DiagnosticFactory.XamlPresentationTypeResolveError(_name, candidates!.Value, _origin));
                _type = null;
                return false;
            }
        }

        private static bool TryResolveQualifiedXamlType(string _name, string? _namespace, CodeOrigin _origin, out QualifiedType? _type)
        {
            // TODO Resolve alias
            _type = new QualifiedType(_namespace is null ? null : Namespace.Parse(_namespace), _name) with { Origin = _origin };
            return true;
        }

        private static bool TryResolveXamlType(TypeToken _token, GeneratorExecutionContext _context, MetadataReference _winUi, string _source, string _sourcePath, out QualifiedType? _type)
        {
            CodeOrigin origin = GetOrigin(_token, _source, _sourcePath);
            return _token.Namespace switch
            {
                "http://schemas.microsoft.com/winfx/2006/xaml/presentation" => TryResolveWinUIXamlType(_token.Name, _context, _winUi, origin, out _type),
                "using:" => TryResolveQualifiedXamlType(_token.Name, null, origin, out _type),
                string ns => TryResolveQualifiedXamlType(_token.Name, ns.Substring("using:".Length), origin, out _type)
            };
        }

        private static void Process(
            Markup _markup,
            GeneratorExecutionContext _context,
            QualifiedType _root,
            ImmutableArray<UsingDirective> _usings,
            NameDictionary _nameDictionary,
            string _source,
            string _sourcePath,
            MetadataReference _winUi,
            IHookInitializer _hookInitializer)
        {
            if (!TryResolveXamlType(_markup.Object, _context, _winUi, _source, _sourcePath, out QualifiedType? objectType))
            {
                return;
            }
            // TODO Resolve return type
            if ((_markup.Code, _markup.Path) is (null, null) or (not null, not null))
            {
                _context.ReportDiagnostic(DiagnosticFactory.XamlCodePropertyError(GetOrigin(_markup.MarkupObject, _source, _sourcePath)));
                return;
            }
            string code;
            if (_markup.Code is not null)
            {
                code = _markup.Code?.Value!;
            }
            else
            {
                string? path = _markup.Path?.Value!;
                try
                {
                    code = File.ReadAllText(path);
                }
                catch
                {
                    _context.ReportDiagnostic(DiagnosticFactory.FileReadError(path));
                    return;
                }
            }
            try
            {
                Script script = Parser.Script(code, _sourcePath);
                bool failed = false;
                if (failed)
                {
                    return;
                }
                script = script.AddUsingDirectives(_usings);
                script = script.AddGlobals(new VC::Emission.Global(true, objectType!, "this"), new VC::Emission.Global(true, _root, "root"));
                ProgramTag codeTag = new("Code", code);
                ProgramTag propertyTag = new("TargetProperty", _markup.Property.Value);
                ProgramTag objectTag = new("TargetObject", objectType with { Alias = null });
                string name = c_namePrefix + _nameDictionary.TakeName(_root.Path.Last().Name);
                VC::Options options = VC::Options.Default with
                {
                    Namespace = new Namespace(_root.Path.SkipLast().Select(_s => _s.Name)),
                    ClassName = name,
                    References = default,
                    HookInitializer = _hookInitializer,
                    ContainingClass = _root.Path.Last().Name,
                    Accessibility = VC::Options.EAccessibility.Private,
                    Tags = ImmutableArray.Create(propertyTag, objectTag, codeTag),
                    ReturnType = "object"
                };
                VC::Compilation compilation = VC::Compilation.SucceedOrThrow(script, options, _context.CancellationToken, (CSharpCompilation)_context.Compilation);
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

        private static void Process(AdditionalText _text, GeneratorExecutionContext _context, ImmutableArray<UsingDirective> _usings, NameDictionary _nameDictionary, MetadataReference _winUi, IHookInitializer _hookInitializer)
        {
            SourceText? sourceText = _text.GetText(_context.CancellationToken);
            if (sourceText is null)
            {
                _context.ReportDiagnostic(DiagnosticFactory.FileReadError(_text.Path));
                return;
            }
            string code = sourceText.ToString();
            ImmutableArray<Markup> markups;
            TypeToken rootToken;
            try
            {
                Parse(code, out rootToken, out markups);
            }
            catch (XamlParseException exception)
            {
                CodeOrigin origin = GetOrigin(exception.LineNumber, exception.LinePosition, code, _text.Path);
                _context.ReportDiagnostic(DiagnosticFactory.XamlParseError(exception.Message, origin));
                return;
            }
            if (markups.Length > 0)
            {
                if (!TryResolveXamlType(rootToken, _context, _winUi, code, _text.Path, out QualifiedType? rootType))
                {
                    return;
                }
                foreach (Markup m in markups)
                {
                    Process(m, _context, rootType!, _usings, _nameDictionary, code, _text.Path, _winUi, _hookInitializer);
                }
            }
            return;
        }

        private static bool TryGetWinUISymbol(GeneratorExecutionContext _context, out MetadataReference? _winUi)
        {
            IEnumerable<MetadataReference> references = ReferenceFinder.OrderByFileNameHint(_context.Compilation.References, "Microsoft.WinUI");
            _winUi = ReferenceFinder.FindByType("Microsoft.UI.Xaml.Window", _context.Compilation, references).FirstOrDefault();
            if (_winUi is null)
            {
                _context.ReportDiagnostic(DiagnosticFactory.NoWinUI());
            }
            return _winUi is not null;
        }

        public void Execute(GeneratorExecutionContext _context)
        {
            if (!UsingsOption.TryGet(_context, out ImmutableArray<UsingDirective> usingDirectives))
            {
                return;
            }
            if (!TryGetWinUISymbol(_context, out MetadataReference? winUi))
            {
                return;
            }
            IHookInitializer hookInitializer = new HookInitializerList(new DependencyPropertyHookInitializer(), new NotifyPropertyChangedHookInitializer());
            NameDictionary nameDictionary = new();
            foreach (AdditionalText text in _context.AdditionalFiles.Where(_f => Path.GetExtension(_f.Path).Equals(".xaml", StringComparison.OrdinalIgnoreCase)))
            {
                if (_context.CancellationToken.IsCancellationRequested)
                {
                    _context.ReportDiagnostic(DiagnosticFactory.Canceled());
                    return;
                }
                Process(text, _context, usingDirectives, nameDictionary, winUi!, hookInitializer);
            }
        }

    }

}
