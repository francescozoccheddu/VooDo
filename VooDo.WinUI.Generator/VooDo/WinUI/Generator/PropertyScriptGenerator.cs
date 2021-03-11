using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Portable.Xaml;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

using VooDo.AST;
using VooDo.AST.Directives;
using VooDo.AST.Names;
using VooDo.Compiling;
using VooDo.Compiling.Emission;
using VooDo.Hooks;
using VooDo.Parsing;
using VooDo.Problems;
using VooDo.Utils;
using VooDo.WinUI.HookInitializers;

using static VooDo.WinUI.Generator.XamlParsing;

using Compilation = VooDo.Compiling.Compilation;

namespace VooDo.WinUI.Generator
{

    [Generator]
    internal sealed class PropertyScriptGenerator : ISourceGenerator
    {

        private readonly struct Markup
        {

            internal static IEqualityComparer<Markup> ValueComparer { get; } = new Comparer();

            private sealed class Comparer : IEqualityComparer<Markup>
            {
                public bool Equals(Markup _x, Markup _y) =>
                    (ReferenceEquals(_x.Code, _y.Code) || (_x.Code is not null && _y.Code is not null && Token.ValueComparer.Equals(_x.Code.Value, _y.Code.Value))) &&
                    (ReferenceEquals(_x.Code, _y.Code) || (_x.Code is not null && _y.Code is not null && Token.ValueComparer.Equals(_x.Code.Value, _y.Code.Value))) &&
                    Token.ValueComparer.Equals(_x.Property, _y.Property) &&
                    TypeToken.ValueComparer.Equals(_x.Object, _y.Object) &&
                    Token.ValueComparer.Equals(_x.MarkupObject, _y.MarkupObject) &&
                    _x.IsObjectBinding == _y.IsObjectBinding;
                public int GetHashCode(Markup _obj) => Identity.CombineHash(_obj.Code, _obj.Path, _obj.Property.Value, _obj.Object.Name, _obj.Object.Namespace, _obj.IsObjectBinding);
            }

            public Markup(Token? _code, Token? _path, Token _property, TypeToken _object, Token _markupObject, bool _isObjectBinding)
            {
                Code = _code;
                Path = _path;
                Property = _property;
                Object = _object;
                MarkupObject = _markupObject;
                IsObjectBinding = _isObjectBinding;
            }

            public Token? Code { get; }
            public Token? Path { get; }
            public Token Property { get; }
            public TypeToken Object { get; }
            public Token MarkupObject { get; }
            public bool IsObjectBinding { get; }

        }


        private static void Parse(string _xaml, out TypeToken _root, out ImmutableArray<Markup> _markups)
        {
            (XamlXmlReader reader, TypeToken root) = GetReaderAndRoot(_xaml);
            _root = root;
            List<Markup> markups = new();
            Token targetProperty = default;
            Stack<TypeToken> targetObjects = new();
            Token? code = null, path = null;
            XamlMember markupProperty = null!;
            Token? markupObject = null;
            bool isObjectBinding = false;
            while (!reader.IsEof)
            {
                reader.Read();
                switch (reader.NodeType)
                {
                    case XamlNodeType.StartObject when reader.Type.Name == Identifiers.PropertyScripts.markupObjectName
                                && reader.Type.PreferredXamlNamespace == Identifiers.xamlUsingNamespacePrefix + Identifiers.PropertyScripts.markupObjectNamespace:
                        markupObject = new(reader.Type.Name, reader.LineNumber - 1, reader.LinePosition - 1);
                        code = path = null;
                        break;
                    case XamlNodeType.StartObject:
                        targetObjects.Push(new(reader.Type.Name, reader.Type.PreferredXamlNamespace, reader.LineNumber - 1, reader.LinePosition - 1));
                        break;
                    case XamlNodeType.EndObject when markupObject is not null:
                        markups.Add(new Markup(code, path, targetProperty, targetObjects.Peek(), markupObject.Value, isObjectBinding));
                        markupObject = null;
                        break;
                    case XamlNodeType.EndObject:
                        if (targetObjects.Count > 0)
                        {
                            targetObjects.Pop();
                        }
                        break;
                    case XamlNodeType.StartMember when markupObject is not null:
                        markupProperty = reader.Member;
                        break;
                    case XamlNodeType.StartMember when reader.Member != XamlLanguage.UnknownContent:
                        string propertyName = reader.Member.Name;
                        isObjectBinding = propertyName is Identifiers.PropertyScripts.attachmentPropertyName or Identifiers.PropertyScripts.attachmentPropertyOwnerName + "." + Identifiers.PropertyScripts.attachmentPropertyName;
                        if (propertyName.StartsWith(Identifiers.PropertyScripts.attachmentPropertyOwnerName + "."))
                        {
                            propertyName = propertyName.Substring(Identifiers.PropertyScripts.attachmentPropertyOwnerName.Length + 1);
                        }
                        targetProperty = new(propertyName, reader.LineNumber - 1, reader.LinePosition - 1);
                        break;
                    case XamlNodeType.Value when markupObject is not null && (markupProperty.Name == Identifiers.PropertyScripts.markupCodePropertyName || markupProperty == XamlLanguage.Initialization):
                        code = new((string)reader.Value, reader.LineNumber - 1, reader.LinePosition - 1);
                        break;
                    case XamlNodeType.Value when markupObject is not null && markupProperty.Name == Identifiers.PropertyScripts.markupPathPropertyName:
                        path = new((string)reader.Value, reader.LineNumber - 1, reader.LinePosition - 1);
                        break;
                }
            }
            _markups = markups.Distinct(Markup.ValueComparer).ToImmutableArray();
        }

        public void Initialize(GeneratorInitializationContext _context) { }

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
                string path = _markup.Path?.Value!;
                string fileDirectory = Path.GetDirectoryName(_sourcePath);
                if (!Path.IsPathRooted(path))
                {
                    path = Path.Combine(fileDirectory, path);
                }
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
                Script script = Parser.ScriptOrExpression(code, _sourcePath);
                bool failed = false;
                if (failed)
                {
                    return;
                }
                script = script.AddUsingDirectives(_usings);
                script = script.AddGlobals(
                    new Global(true, objectType!, Identifiers.PropertyScripts.thisVariableName),
                    new Global(true, _root, Identifiers.PropertyScripts.rootVariableName));
                ProgramTag codeTag = new(Identifiers.PropertyScripts.codeTag, _markup.Code?.Value);
                ProgramTag propertyTag = new(Identifiers.PropertyScripts.propertyTag, _markup.Property.Value);
                ProgramTag pathTag = new(Identifiers.PropertyScripts.pathTag, _markup.Path?.Value);
                ProgramTag xamlPathTag = new(Identifiers.PropertyScripts.xamlPathTag, FilePaths.Normalize(_sourcePath));
                ProgramTag objectTag = new(Identifiers.PropertyScripts.objectTag, objectType! with { Alias = null });
                string name = Identifiers.PropertyScripts.scriptPrefix + _nameDictionary.TakeName(_root.Path.Last().Name);
                Options options = Options.Default with
                {
                    Namespace = new Namespace(_root.Path.SkipLast().Select(_s => _s.Name)),
                    ClassName = name,
                    References = default,
                    HookInitializer = _hookInitializer,
                    ContainingClass = _root.Path.Last().Name,
                    Accessibility = Options.EAccessibility.Private,
                    Tags = ImmutableArray.Create(propertyTag, objectTag, codeTag, pathTag, xamlPathTag),
                    ReturnType = _markup.IsObjectBinding ? null : ComplexType.Parse("object")
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

        public void Execute(GeneratorExecutionContext _context)
        {
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                //System.Diagnostics.Debugger.Launch();
            }
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
            foreach (AdditionalText text in _context.AdditionalFiles.Where(_f => Path.GetExtension(_f.Path).Equals(Identifiers.xamlFileExtension, FilePaths.SystemComparison)))
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
