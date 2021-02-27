using Microsoft.CodeAnalysis;
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
using VooDo.Utils;

namespace VooDo.Generator
{

    [Generator]
    internal sealed class PropertyScriptGenerator : ISourceGenerator
    {

        private readonly struct Markup
        {

            public Markup(Token? _code, Token? _path, Token? _tag, Token _property, TypeToken _object)
            {
                Code = _code;
                Path = _path;
                Tag = _tag;
                Property = _property;
                Object = _object;
            }

            public Token? Code { get; }
            public Token? Path { get; }
            public Token? Tag { get; }
            public Token Property { get; }
            public TypeToken Object { get; }

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
            bool isMarkup = false;
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
                        isMarkup = true;
                        code = tag = path = null;
                        break;
                    case XamlNodeType.StartObject:
                        targetObject = new TypeToken(reader.Type.Name, reader.Type.PreferredXamlNamespace, reader.LineNumber - 1, reader.LinePosition - 1);
                        break;
                    case XamlNodeType.EndObject when isMarkup:
                        markups.Add(new Markup(code, path, tag, targetProperty, targetObject));
                        isMarkup = false;
                        break;
                    case XamlNodeType.StartMember when isMarkup:
                        markupProperty = reader.Member;
                        break;
                    case XamlNodeType.StartMember when reader.Member != XamlLanguage.UnknownContent:
                        targetProperty = new Token(reader.Member.Name, reader.LineNumber - 1, reader.LinePosition - 1);
                        break;
                    case XamlNodeType.Value when isMarkup && (markupProperty.Name == "Code" || markupProperty == XamlLanguage.Initialization):
                        code = new Token((string)reader.Value, reader.LineNumber - 1, reader.LinePosition - 1);
                        break;
                    case XamlNodeType.Value when isMarkup && markupProperty.Name == "Tag":
                        tag = new Token((string)reader.Value, reader.LineNumber - 1, reader.LinePosition - 1);
                        break;
                    case XamlNodeType.Value when isMarkup && markupProperty.Name == "Path":
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

        private static CodeOrigin GetOrigin(int _line, int _column, string _source, int _length = 0)
            => new CodeOrigin(GetCharacterIndex(_line, _column, _source), _length, _source);

        private static CodeOrigin GetOrigin(Token _token, string _source)
            => new CodeOrigin(GetCharacterIndex(_token.Line, _token.Column, _source), _token.Length, _source);

        private static CodeOrigin GetOrigin(TypeToken _token, string _source)
            => new CodeOrigin(GetCharacterIndex(_token.Line, _token.Column, _source), _token.Length, _source);

        private static bool TryResolveWinUIXamlType(string _name, GeneratorExecutionContext _context, MetadataReference _winUi, (CodeOrigin, string) _origin, out QualifiedType? _type)
        {
            ImmutableArray<QualifiedType>? candidates = ReferenceFinder.FindTypeByPartialName(_name, _context.Compilation, _winUi);
            if (!candidates.HasValue)
            {
                _context.ReportDiagnostic(DiagnosticFactory.NoWinUI());
                _type = null;
                return false;
            }
            if (candidates?.Length == 1)
            {
                _type = candidates?[0];
                return true;
            }
            else
            {
                _context.ReportDiagnostic(DiagnosticFactory.XamlPresentationTypeResolveError(_name, candidates!.Value, _origin.Item1, _origin.Item2));
                _type = null;
                return false;
            }
        }

        private static bool TryResolveQualifiedXamlType(string _name, string? _namespace, (CodeOrigin, string) _origin, out QualifiedType? _type)
        {
            // TODO Resolve alias
            _type = new QualifiedType(_namespace is null ? null : Namespace.Parse(_namespace), _name);
            return true;
        }

        private static bool TryResolveXamlType(TypeToken _token, GeneratorExecutionContext _context, MetadataReference _winUi, string _source, string _sourcePath, out QualifiedType? _type)
        {
            (CodeOrigin, string) origin = (GetOrigin(_token, _source), _sourcePath);
            return _token.Namespace switch
            {
                "http://schemas.microsoft.com/winfx/2006/xaml/presentation" => TryResolveWinUIXamlType(_token.Name, _context, _winUi, origin, out _type),
                "using:" => TryResolveQualifiedXamlType(_token.Name, null, origin, out _type),
                string ns => TryResolveQualifiedXamlType(_token.Name, ns.Substring("using:".Length), origin, out _type)
            };
        }

        private static void Process(AdditionalText _text, GeneratorExecutionContext _context, ImmutableArray<UsingDirective> _usings, NameDictionary _nameDictionary, MetadataReference _winUi)
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
                CodeOrigin origin = GetOrigin(exception.LineNumber, exception.LinePosition, code);
                _context.ReportDiagnostic(DiagnosticFactory.XamlParseError(_text.Path, exception.Message, origin));
                return;
            }
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Launch();
            }
            if (markups.Length > 0)
            {
                if (!TryResolveXamlType(rootToken, _context, _winUi, code, _text.Path, out QualifiedType? rootType))
                {
                    return;
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
            NameDictionary nameDictionary = new();
            foreach (AdditionalText text in _context.AdditionalFiles.Where(_f => Path.GetExtension(_f.Path).Equals(".xaml", StringComparison.OrdinalIgnoreCase)))
            {
                if (_context.CancellationToken.IsCancellationRequested)
                {
                    _context.ReportDiagnostic(DiagnosticFactory.Canceled());
                    return;
                }
                Process(text, _context, usingDirectives, nameDictionary, winUi!);
            }
        }

    }

}
