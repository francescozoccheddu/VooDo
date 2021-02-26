using Microsoft.CodeAnalysis;

using Portable.Xaml;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

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

        }

        private readonly struct TypeToken
        {

            public TypeToken(string _name, string _namespace, int _line, int _column)
            {
                Name = _name;
                Namespace = _namespace;
                Line = _line;
                Column = _column;
            }

            public string Name { get; }
            public string Namespace { get; }
            public int Line { get; }
            public int Column { get; }

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
                            _root = new TypeToken(reader.Type.Name, reader.Type.PreferredXamlNamespace, reader.LineNumber, reader.LinePosition);
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
                            _root = new TypeToken(name, nameSpace, reader.LineNumber, reader.LinePosition);
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
                        targetObject = new TypeToken(reader.Type.Name, reader.Type.PreferredXamlNamespace, reader.LineNumber, reader.LinePosition);
                        break;
                    case XamlNodeType.EndObject when isMarkup:
                        markups.Add(new Markup(code, path, tag, targetProperty, targetObject));
                        isMarkup = false;
                        break;
                    case XamlNodeType.StartMember when isMarkup:
                        markupProperty = reader.Member;
                        break;
                    case XamlNodeType.StartMember when reader.Member != XamlLanguage.UnknownContent:
                        targetProperty = new Token(reader.Member.Name, reader.LineNumber, reader.LinePosition);
                        break;
                    case XamlNodeType.Value when isMarkup && (markupProperty.Name == "Code" || markupProperty == XamlLanguage.Initialization):
                        code = new Token((string)reader.Value, reader.LineNumber, reader.LinePosition);
                        break;
                    case XamlNodeType.Value when isMarkup && markupProperty.Name == "Tag":
                        tag = new Token((string)reader.Value, reader.LineNumber, reader.LinePosition);
                        break;
                    case XamlNodeType.Value when isMarkup && markupProperty.Name == "Path":
                        path = new Token((string)reader.Value, reader.LineNumber, reader.LinePosition);
                        break;
                }
            }
            _markups = markups.ToImmutableArray();
        }

        public void Initialize(GeneratorInitializationContext _context) { }

        public void Execute(GeneratorExecutionContext _context)
        {

        }

    }

}
