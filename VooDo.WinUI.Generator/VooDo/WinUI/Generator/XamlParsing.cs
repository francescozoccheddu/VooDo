using Microsoft.CodeAnalysis;

using Portable.Xaml;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

using VooDo.AST;
using VooDo.AST.Names;
using VooDo.Utils;

namespace VooDo.WinUI.Generator
{

    internal static class XamlParsing
    {

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

        internal static CodeOrigin GetOrigin(int _line, int _column, string _source, string _sourcePath, int _length = 0)
            => new(GetCharacterIndex(_line, _column, _source), _length, _source, _sourcePath);

        internal static CodeOrigin GetOrigin(Token _token, string _source, string _sourcePath)
            => new(GetCharacterIndex(_token.Line, _token.Column, _source), _token.Length, _source, _sourcePath);

        internal static CodeOrigin GetOrigin(TypeToken _token, string _source, string _sourcePath)
            => new(GetCharacterIndex(_token.Line, _token.Column, _source), _token.Length, _source, _sourcePath);

        private static bool TryResolveWinUIXamlType(string _name, GeneratorExecutionContext _context, MetadataReference _winUi, CodeOrigin _origin, out QualifiedType? _type)
        {
            int presentationNamespaceDepth = Identifiers.xamlPresentationBaseNamespace.Count(_c => _c == '.') + 1;
            ImmutableArray<QualifiedType>? candidates = ReferenceFinder.FindTypeByPartialName(_name, _context.Compilation, _winUi)
                ?.Where(_c => _c.Path.Length > presentationNamespaceDepth
                && string.Join(".", _c.Path.Take(presentationNamespaceDepth)) == Identifiers.xamlPresentationBaseNamespace)
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

        internal static bool TryResolveXamlType(TypeToken _token, GeneratorExecutionContext _context, MetadataReference _winUi, string _source, string _sourcePath, out QualifiedType? _type)
        {
            CodeOrigin origin = GetOrigin(_token, _source, _sourcePath);
            return _token.Namespace switch
            {
                Identifiers.xamlPresentationNamespace => TryResolveWinUIXamlType(_token.Name, _context, _winUi, origin, out _type),
                Identifiers.xamlUsingNamespacePrefix => TryResolveQualifiedXamlType(_token.Name, null, origin, out _type),
                string ns => TryResolveQualifiedXamlType(_token.Name, ns.Substring(Identifiers.xamlUsingNamespacePrefix.Length), origin, out _type)
            };
        }

        internal static bool TryGetWinUISymbol(GeneratorExecutionContext _context, out MetadataReference? _winUi)
        {
            IEnumerable<MetadataReference> references = ReferenceFinder.OrderByFileNameHint(_context.Compilation.References, Identifiers.winUiName);
            _winUi = ReferenceFinder.FindByType(Identifiers.xamlPresentationBaseNamespace + ".Window", _context.Compilation, references).FirstOrDefault();
            if (_winUi is null)
            {
                _context.ReportDiagnostic(DiagnosticFactory.NoWinUI());
            }
            return _winUi is not null;
        }

        internal readonly struct Token
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

        internal readonly struct TypeToken
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

        internal static XamlXmlReader GetReader(string _xaml)
        {
            _xaml = _xaml.Replace("x:DataType", "__notSupportedDataTypeProperty");
            return new(new StringReader(_xaml));
        }

        internal static TypeToken GetRoot(XamlXmlReader _reader)
        {
            TypeToken root = default;
            bool isRootSet = false;
            bool isRootClass = false;
            bool isRootConfirmed = false;
            while (!_reader.IsEof)
            {
                _reader.Read();
                if (!isRootConfirmed)
                {
                    switch (_reader.NodeType)
                    {
                        case XamlNodeType.StartObject when !isRootSet:
                            root = new TypeToken(_reader.Type.Name, _reader.Type.PreferredXamlNamespace, _reader.LineNumber - 1, _reader.LinePosition - 1);
                            isRootSet = true;
                            break;
                        case XamlNodeType.StartObject:
                            isRootConfirmed = true;
                            break;
                        case XamlNodeType.StartMember when _reader.Member == XamlLanguage.Class:
                            isRootClass = true;
                            break;
                        case XamlNodeType.Value when isRootClass:
                            string[] tokens = ((string)_reader.Value).Split('.');
                            string name = tokens.Last();
                            string nameSpace = Identifiers.xamlUsingNamespacePrefix + string.Join(".", tokens.SkipLast());
                            root = new TypeToken(name, nameSpace, _reader.LineNumber - 1, _reader.LinePosition - 1, 0);
                            isRootConfirmed = true;
                            break;
                    }
                }
            }
            return root;
        }

        internal static (XamlXmlReader reader, TypeToken root) GetReaderAndRoot(string _xaml)
        {
            XamlXmlReader reader = GetReader(_xaml);
            TypeToken root = GetRoot(reader);
            reader.Close();
            reader = GetReader(_xaml);
            return (reader, root);
        }
    }

}
