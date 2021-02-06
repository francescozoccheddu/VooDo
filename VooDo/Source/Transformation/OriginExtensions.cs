#nullable enable

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

using System;
using System.Linq;

using VooDo.Factory;

namespace VooDo.Transformation
{

    public static class OriginExtensions
    {

        public const string annotationKind = "VooDo: " + nameof(OriginExtensions) + " Annotation - OriginalSpan";

        public static Origin FromSpan(TextSpan _span)
            => Origin.FromSource(_span.Start, _span.Length);

        public static SyntaxToken WithOrigin(this SyntaxToken _node, Origin? _origin)
        {
            SyntaxAnnotation? annotation = _node.GetOriginAnnotation();
            if (!(annotation == null))
            {
                _node = _node.WithoutAnnotations(annotation);
            }
            if (_origin != null)
            {
                _node = _node.WithAdditionalAnnotations(_origin.Value.CreateAnnotation());
            }
            return _node;
        }

        public static TNode WithOrigin<TNode>(this TNode _node, Origin? _origin, bool _recursive) where TNode : SyntaxNode
            => _recursive ? OriginRewriter.RewriteAbsolute(_node, _origin) : _node.WithOrigin(_origin);

        public static TNode WithOrigin<TNode>(this TNode _node, Origin? _origin) where TNode : SyntaxNode
        {
            SyntaxAnnotation? annotation = _node.GetOriginAnnotation();
            if (!(annotation == null))
            {
                _node = _node.WithoutAnnotations(annotation);
            }
            if (_origin != null)
            {
                _node = _node.WithAdditionalAnnotations(_origin.Value.CreateAnnotation());
            }
            return _node;
        }

        public static Origin GetOrigin(this SyntaxToken _token)
            => _token.TryGetOrigin().GetValueOrDefault();

        public static Origin? TryGetOrigin(this SyntaxToken _token)
        {
            SyntaxAnnotation? annotation = _token.GetOriginAnnotation();
            return annotation == null ? null : FromAnnotation(annotation);
        }

        public static Origin GetOrigin(this SyntaxNode _node)
            => _node.TryGetOrigin().GetValueOrDefault();

        public static Origin? TryGetOrigin(this SyntaxNode _node)
        {
            SyntaxAnnotation? annotation = _node.GetOriginAnnotation();
            return annotation == null ? null : FromAnnotation(annotation);
        }

        public static SyntaxAnnotation? GetOriginAnnotation(this SyntaxToken _node)
        {
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

        public static SyntaxAnnotation? GetOriginAnnotation(this SyntaxNode _node)
        {
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

        public static Origin FromAnnotation(SyntaxAnnotation _annotation)
        {
            if (_annotation.Kind != annotationKind)
            {
                throw new ArgumentException("Unexpected annotation kind");
            }
            return Deserialize(_annotation.Data!);
        }

        public static SyntaxAnnotation CreateAnnotation(this Origin _origin)
            => new SyntaxAnnotation(annotationKind, _origin.Serialize());

        public static string Serialize(this Origin _origin)
            => _origin.Kind switch
            {
                Origin.EKind.Source => $"{(int) _origin.Kind};{_origin.Start};{_origin.Length}",
                _ => $"{(int) _origin.Kind};;"
            };

        public static Origin Deserialize(string _serializedSpan)
        {
            string[] tokens = _serializedSpan.Split(';');
            if (tokens.Length != 3)
            {
                throw new FormatException("Expected three semicolon-separated tokens");
            }
            Origin.EKind kind = (Origin.EKind) int.Parse(tokens[0]);
            if (kind != Origin.EKind.Source && !(string.IsNullOrWhiteSpace(tokens[1]) && string.IsNullOrWhiteSpace(tokens[2])))
            {
                throw new FormatException("Unexpected start or length token after non source kind");
            }
            return kind switch
            {
                Origin.EKind.Source => Origin.FromSource(int.Parse(tokens[1]), int.Parse(tokens[2])),
                Origin.EKind.Transformation => Origin.Transformation,
                Origin.EKind.HookInitializer => Origin.HookInitializer,
                _ => default
            };
        }

    }

}
