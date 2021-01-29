using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

using System;
using System.Linq;

namespace VooDo.Transformation
{

    public static class TextSpanExtensions
    {

        public const string annotationKind = "VooDo: " + nameof(TextSpanExtensions) + " Annotation";

        public static TNode WithSpan<TNode>(this TNode _node, TextSpan? _span) where TNode : SyntaxNode
        {
            if (_node == null)
            {
                throw new ArgumentNullException(nameof(_node));
            }
            SyntaxAnnotation annotation = _node.GetSpanAnnotation();
            if (annotation != null)
            {
                _node = _node.WithoutAnnotations(annotation);
            }
            if (_span != null)
            {
                _node = _node.WithAdditionalAnnotations(_span.Value.CreateAnnotation());
            }
            return _node;
        }

        public static TextSpan GetSpan(this SyntaxNode _node)
            => _node.TryGetSpan().Value;

        public static TextSpan? TryGetSpan(this SyntaxNode _node)
        {
            if (_node == null)
            {
                throw new ArgumentNullException(nameof(_node));
            }
            SyntaxAnnotation annotation = _node.GetSpanAnnotation();
            return annotation != null ? FromAnnotation(annotation) : (TextSpan?) null;
        }

        public static SyntaxAnnotation GetSpanAnnotation(this SyntaxNode _node)
        {
            if (_node == null)
            {
                throw new ArgumentNullException(nameof(_node));
            }
            SyntaxAnnotation[] annotations = _node.GetAnnotations(annotationKind).ToArray();
            if (annotations.Length == 0)
            {
                return null;
            }
            if (annotations.Length > 1)
            {
                throw new ArgumentException("Node has multiple annotations", nameof(_node));
            }
            return annotations[0];
        }

        public static TextSpan FromAnnotation(SyntaxAnnotation _annotation)
        {
            if (_annotation == null)
            {
                throw new ArgumentNullException(nameof(_annotation));
            }
            if (_annotation.Kind != annotationKind)
            {
                throw new ArgumentException("Unexpected annotation kind");
            }
            return Deserialize(_annotation.Data);
        }

        public static SyntaxAnnotation CreateAnnotation(this TextSpan _span)
            => new SyntaxAnnotation(annotationKind, _span.Serialize());

        public static string Serialize(this TextSpan _span)
            => $"{_span.Start};{_span.Length}";

        public static TextSpan Deserialize(string _serializedSpan)
        {
            if (_serializedSpan == null)
            {
                throw new ArgumentNullException(nameof(_serializedSpan));
            }
            string[] tokens = _serializedSpan.Split(';');
            if (tokens.Length != 2)
            {
                throw new FormatException("Expected two semicolon-separated tokens");
            }
            int start = int.Parse(tokens[0]);
            int length = int.Parse(tokens[1]);
            return new TextSpan(start, length);
        }

        public static TextSpan Offset(this TextSpan _span, int _offset)
            => new TextSpan(_span.Start + _offset, _span.Length);

    }

}
