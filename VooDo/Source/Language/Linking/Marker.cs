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

        private readonly Dictionary<BodyNode, int> m_forward = new Dictionary<BodyNode, int>();
        private readonly List<BodyNode> m_reverse = new List<BodyNode>();

        private int GetOwnerIndex(BodyNode _node)
        {
            if (!m_forward.TryGetValue(_node, out int index))
            {
                index = m_forward.Count;
                m_forward.Add(_node, index);
                m_reverse.Add(_node);
            }
            return index;
        }

        internal TNode Own<TNode>(TNode _node, BodyNode _owner, EMode _mode = EMode.Single) where TNode : SyntaxNode
            => _mode switch
            {
                EMode.Single => (TNode) SetIndex(_node, GetOwnerIndex(_owner), true)?.AsNode()!,
                EMode.AllDescendants => (TNode) new IndexRewriter(GetOwnerIndex(_owner), true).Visit(_node)!,
                EMode.UnownedDescendants => (TNode) new IndexRewriter(GetOwnerIndex(_owner), false).Visit(_node)!,
                _ => throw new ArgumentOutOfRangeException(nameof(_mode)),
            };

        internal SyntaxToken Own(SyntaxToken _token, BodyNode _owner)
            => SetIndex(_token, GetOwnerIndex(_owner), true)!.Value.AsToken();

        internal SyntaxNodeOrToken Own(SyntaxNodeOrToken _nodeOrToken, BodyNode _owner, EMode _mode = EMode.Single)
            => _nodeOrToken.IsToken
            ? Own(_nodeOrToken.AsToken(), _owner)
            : Own(_nodeOrToken.AsNode()!, _owner, _mode);

        internal BodyNode GetOwner(SyntaxNodeOrToken _nodeOrToken)
            => m_reverse[int.Parse(GetAnnotation(_nodeOrToken)!.Data!)];

        internal NodeMarker ForOwner(BodyNode _node)
            => new NodeMarker(this, _node);

    }

    internal sealed class NodeMarker
    {

        internal Marker Marker { get; }
        internal BodyNode Owner { get; }

        public NodeMarker(Marker _marker, BodyNode _owner)
        {
            Owner = _owner;
            Marker = _marker;
        }

        internal TNode Own<TNode>(TNode _node, Marker.EMode _mode = Marker.EMode.Single) where TNode : SyntaxNode
            => Marker.Own(_node, Owner, _mode);

        internal SyntaxToken Own(SyntaxToken _token)
            => Marker.Own(_token, Owner);

        internal SyntaxNodeOrToken Own(SyntaxNodeOrToken _nodeOrToken, Marker.EMode _mode = Marker.EMode.Single)
            => Marker.Own(_nodeOrToken, Owner, _mode);

    }

}
