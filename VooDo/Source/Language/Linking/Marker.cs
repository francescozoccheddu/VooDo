using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using System;
using System.Collections.Generic;
using System.Linq;

using VooDo.Language.AST;

namespace VooDo.Language.Linking
{

    internal sealed class Marker
    {

        private const string c_annotationKind = "VooDo " + nameof(Marker);
        private static SyntaxAnnotation CreateAnnotation(int _index)
            => new SyntaxAnnotation(c_annotationKind, _index.ToString());
        private static SyntaxAnnotation? GetAnnotation(SyntaxNodeOrToken _nodeOrToken)
            => _nodeOrToken.GetAnnotations(c_annotationKind).SingleOrDefault();
        private static SyntaxNodeOrToken? SetIndex(SyntaxNodeOrToken _node, int _index, bool _overwrite)
        {
            SyntaxAnnotation? annotation = GetAnnotation(_node);
            if (annotation is not null)
            {
                if (!_overwrite)
                {
                    return null;
                }
                _node = _node.WithoutAnnotations(annotation);
            }
            return _node.WithAdditionalAnnotations(CreateAnnotation(_index));
        }

        private sealed class IndexRewriter : CSharpSyntaxRewriter
        {

            private readonly bool m_overwrite;
            private readonly int m_index;

            public IndexRewriter(int _index, bool _overwrite)
            {
                m_index = _index;
                m_overwrite = _overwrite;
            }

            public override SyntaxNode? Visit(SyntaxNode? _node)
            {
                if (_node != null)
                {
                    SyntaxNodeOrToken? result = SetIndex(_node, m_index, m_overwrite);
                    if (result != null)
                    {
                        return result?.AsNode()!;
                    }
                    else if (!m_overwrite)
                    {
                        return _node;
                    }
                }
                return base.Visit(_node);
            }

            public override SyntaxToken VisitToken(SyntaxToken _token)
            {
                SyntaxNodeOrToken? result = SetIndex(_token, m_index, m_overwrite);
                if (result != null)
                {
                    return result!.Value.AsToken();
                }
                else if (!m_overwrite)
                {
                    return _token;
                }
                return base.VisitToken(_token);
            }

        }

        internal enum EMode
        {
            Single, AllDescendants, UnownedDescendants
        }

        private readonly Dictionary<BodyNodeOrIdentifier, int> m_forward = new Dictionary<BodyNodeOrIdentifier, int>();
        private readonly List<BodyNodeOrIdentifier> m_reverse = new List<BodyNodeOrIdentifier>();

        private int GetOwnerIndex(BodyNodeOrIdentifier _node)
        {
            if (!m_forward.TryGetValue(_node, out int index))
            {
                index = m_forward.Count;
                m_forward.Add(_node, index);
                m_reverse.Add(_node);
            }
            return index;
        }

        internal TNode Own<TNode>(TNode _node, BodyNodeOrIdentifier _owner, EMode _mode = EMode.UnownedDescendants) where TNode : SyntaxNode
            => _mode switch
            {
                EMode.Single => (TNode) SetIndex(_node, GetOwnerIndex(_owner), true)?.AsNode()!,
                EMode.AllDescendants => (TNode) new IndexRewriter(GetOwnerIndex(_owner), true).Visit(_node)!,
                EMode.UnownedDescendants => (TNode) new IndexRewriter(GetOwnerIndex(_owner), false).Visit(_node)!,
                _ => throw new ArgumentOutOfRangeException(nameof(_mode)),
            };

        internal SyntaxToken Own(SyntaxToken _token, BodyNodeOrIdentifier _owner)
            => SetIndex(_token, GetOwnerIndex(_owner), true)!.Value.AsToken();

        internal SyntaxNodeOrToken Own(SyntaxNodeOrToken _nodeOrToken, BodyNodeOrIdentifier _owner, EMode _mode = EMode.UnownedDescendants)
            => _nodeOrToken.IsToken
            ? Own(_nodeOrToken.AsToken(), _owner)
            : Own(_nodeOrToken.AsNode()!, _owner, _mode);

        internal BodyNodeOrIdentifier GetOwner(SyntaxNodeOrToken _nodeOrToken)
            => m_reverse[int.Parse(GetAnnotation(_nodeOrToken)!.Data!)];


    }

    internal static class MarkerExtensions
    {

        internal static TNode Own<TNode>(this TNode _node, Marker _marker, BodyNodeOrIdentifier _owner, Marker.EMode _mode = Marker.EMode.UnownedDescendants) where TNode : SyntaxNode
            => _marker.Own(_node, _owner, _mode);

        internal static SyntaxToken Own(this SyntaxToken _token, Marker _marker, BodyNodeOrIdentifier _owner)
            => _marker.Own(_token, _owner);

        internal static SyntaxNodeOrToken Own(this SyntaxNodeOrToken _nodeOrToken, Marker _marker, BodyNodeOrIdentifier _owner, Marker.EMode _mode = Marker.EMode.UnownedDescendants)
            => _marker.Own(_nodeOrToken, _owner, _mode);

    }

}
