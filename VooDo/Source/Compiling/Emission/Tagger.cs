using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using System;
using System.Collections.Generic;
using System.Linq;

using VooDo.AST;
using VooDo.Utils;

namespace VooDo.Compiling.Emission
{

    internal sealed class Tagger
    {

        internal sealed class Tag : IEquatable<Tag?>
        {

            public static Tag FromOwner(Tagger _tagger, Node _owner)
                => new Tag(_tagger.GetOwnerIndex(_owner));

            public static Tag? FromSyntax(SyntaxNodeOrToken _syntax)
            {
                int index = GetIndex(_syntax);
                return index > 0 ? new Tag(index) : null;
            }

            private readonly int m_index;

            private Tag(int _index)
            {
                m_index = _index;
            }

            internal TNode Own<TNode>(TNode _node, EMode _mode = EMode.UnownedDescendants) where TNode : SyntaxNode
                => Tagger.Own(_node, m_index, _mode);

            internal SyntaxToken Own(SyntaxToken _token)
                => Tagger.Own(_token, m_index);

            internal SyntaxNodeOrToken Own(SyntaxNodeOrToken _nodeOrToken, EMode _mode = EMode.UnownedDescendants)
                => Tagger.Own(_nodeOrToken, m_index, _mode);

            internal Node? GetOwner(Tagger _tagger)
                => _tagger.GetOwner(m_index);

            public override bool Equals(object? _obj) => Equals(_obj as Tag);
            public bool Equals(Tag? _other) => _other is not null && m_index == _other.m_index;
            public override int GetHashCode() => m_index.GetHashCode();

            public static bool operator ==(Tag? _left, Tag? _right) => _left is not null && _left.Equals(_right);
            public static bool operator !=(Tag? _left, Tag? _right) => !(_left == _right);

        }

        private const string c_annotationKind = "VooDo " + nameof(Tagger);
        private static SyntaxAnnotation CreateAnnotation(int _index)
            => new SyntaxAnnotation(c_annotationKind, _index.ToString());
        private static SyntaxAnnotation? GetAnnotation(SyntaxNodeOrToken _nodeOrToken)
            => _nodeOrToken.GetAnnotations(c_annotationKind).SingleOrDefault();
        private static int GetIndex(SyntaxNodeOrToken _nodeOrToken)
            => int.TryParse(GetAnnotation(_nodeOrToken)?.Data, out int index) ? index : -1;
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
        private static TNode Own<TNode>(TNode _node, int _index, EMode _mode = EMode.UnownedDescendants) where TNode : SyntaxNode
            => _mode switch
            {
                EMode.Single => (TNode) SetIndex(_node, _index, true)?.AsNode()!,
                EMode.AllDescendants => (TNode) new IndexRewriter(_index, true).Visit(_node)!,
                EMode.UnownedDescendants => (TNode) new IndexRewriter(_index, false).Visit(_node)!,
                _ => throw new ArgumentOutOfRangeException(nameof(_mode)),
            };
        private static SyntaxToken Own(SyntaxToken _token, int _index)
            => SetIndex(_token, _index, true)!.Value.AsToken();
        private static SyntaxNodeOrToken Own(SyntaxNodeOrToken _nodeOrToken, int _index, EMode _mode = EMode.UnownedDescendants)
            => _nodeOrToken.IsToken
            ? Own(_nodeOrToken.AsToken(), _index)
            : Own(_nodeOrToken.AsNode()!, _index, _mode);

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

        private readonly Dictionary<Node, int> m_forward = new Dictionary<Node, int>(new ReferenceComparer<Node>());
        private readonly List<Node> m_reverse = new List<Node>();

        private int GetOwnerIndex(Node _owner)
        {
            if (!m_forward.TryGetValue(_owner, out int index))
            {
                index = m_forward.Count;
                m_forward.Add(_owner, index);
                m_reverse.Add(_owner);
            }
            return index;
        }

        private Node? GetOwner(int _index)
            => _index >= 0 && _index < m_reverse.Count ? m_reverse[_index] : null;

        internal TNode Own<TNode>(TNode _node, Node _owner, EMode _mode = EMode.UnownedDescendants) where TNode : SyntaxNode
            => Own(_node, GetOwnerIndex(_owner), _mode);

        internal SyntaxToken Own(SyntaxToken _token, Node _owner)
            => Own(_token, GetOwnerIndex(_owner));

        internal SyntaxNodeOrToken Own(SyntaxNodeOrToken _nodeOrToken, Node _owner, EMode _mode = EMode.UnownedDescendants)
            => Own(_nodeOrToken, GetOwnerIndex(_owner), _mode);

        internal Node? GetOwner(SyntaxNodeOrToken _nodeOrToken)
            => GetOwner(GetIndex(_nodeOrToken));

    }

    internal static class MarkerExtensions
    {

        internal static TNode Own<TNode>(this TNode _node, Tagger.Tag _tag, Tagger.EMode _mode = Tagger.EMode.UnownedDescendants) where TNode : SyntaxNode
            => _tag.Own(_node, _mode);

        internal static SyntaxToken Own(this SyntaxToken _token, Tagger.Tag _tag)
            => _tag.Own(_token);

        internal static SyntaxNodeOrToken Own(this SyntaxNodeOrToken _nodeOrToken, Tagger.Tag _tag, Tagger.EMode _mode = Tagger.EMode.UnownedDescendants)
            => _tag.Own(_nodeOrToken, _mode);

        internal static TNode Own<TNode>(this TNode _node, Tagger _tagger, Node _owner, Tagger.EMode _mode = Tagger.EMode.UnownedDescendants) where TNode : SyntaxNode
            => _tagger.Own(_node, _owner, _mode);

        internal static SyntaxToken Own(this SyntaxToken _token, Tagger _tagger, Node _owner)
            => _tagger.Own(_token, _owner);

        internal static SyntaxNodeOrToken Own(this SyntaxNodeOrToken _nodeOrToken, Tagger _tagger, Node _owner, Tagger.EMode _mode = Tagger.EMode.UnownedDescendants)
            => _tagger.Own(_nodeOrToken, _owner, _mode);

        internal static Node? GetOwner(this SyntaxNode _node, Tagger _tagger)
            => _tagger.GetOwner(_node);

        internal static Node? GetOwner(this SyntaxToken _token, Tagger _tagger)
            => _tagger.GetOwner(_token);

        internal static Node? GetOwner(this SyntaxNodeOrToken _nodeOrToken, Tagger _tagger)
            => _tagger.GetOwner(_nodeOrToken);

        internal static Tagger.Tag? GetTag(this SyntaxNode _node)
            => Tagger.Tag.FromSyntax(_node);

        internal static Tagger.Tag? GetTag(this SyntaxToken _token)
            => Tagger.Tag.FromSyntax(_token);

        internal static Tagger.Tag? GetTag(this SyntaxNodeOrToken _nodeOrToken)
            => Tagger.Tag.FromSyntax(_nodeOrToken);

        internal static Tagger.Tag? GetTag(this Node _owner, Tagger _tagger)
            => Tagger.Tag.FromOwner(_tagger, _owner);

    }

}
