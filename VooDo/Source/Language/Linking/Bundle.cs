using Microsoft.CodeAnalysis;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Language.AST;

namespace VooDo.Language.Linking
{

    internal sealed class Bundle
    {

        private const string c_annotationKind = "VooDo " + nameof(Bundle);

        private static SyntaxAnnotation CreateAnnotation(int _index) => new SyntaxAnnotation(c_annotationKind, _index.ToString());
        private static int GetAnnotationIndex(SyntaxAnnotation _annotation) => int.Parse(_annotation.Data!);
        private static IEnumerable<int> GetIndices(SyntaxNode _node) => _node.GetAnnotations(c_annotationKind).Select(GetAnnotationIndex);

        internal sealed class Dealer
        {

            internal Dealer() : this(ImmutableDictionary.Create<BodyNode, ImmutableArray<SyntaxNode>>())
            { }

            private Dealer(ImmutableDictionary<BodyNode, ImmutableArray<SyntaxNode>> _previousData)
            {
                m_previousData = _previousData;
            }

            private readonly ImmutableDictionary<BodyNode, ImmutableArray<SyntaxNode>> m_previousData;
            private readonly Dictionary<BodyNode, Bundle> m_bundles = new Dictionary<BodyNode, Bundle>();

            internal Bundle this[BodyNode _node]
            {
                get
                {
                    if (m_bundles.TryGetValue(_node, out Bundle? bundle))
                    {
                        return bundle;
                    }
                    else
                    {
                        ImmutableArray<SyntaxNode> previousData = m_previousData.GetValueOrDefault(_node);
                        bundle = new Bundle(_node, previousData, m_bundles.Count);
                        m_bundles.Add(_node, bundle);
                        return bundle;
                    }
                }
            }

            internal Dealer GetUpdated(SyntaxNode _newRoot)
            {
                Dictionary<BodyNode, List<SyntaxNode>> newData = new Dictionary<BodyNode, List<SyntaxNode>>();
                ImmutableArray<BodyNode> bundleMap = m_bundles.OrderBy(_e => _e.Value.m_index).Select(_e => _e.Key).ToImmutableArray();
                foreach (SyntaxNode syntax in _newRoot.GetAnnotatedNodes(c_annotationKind))
                {
                    foreach (int index in GetIndices(syntax))
                    {
                        BodyNode owner = bundleMap[index];
                        if (!newData.TryGetValue(owner, out List<SyntaxNode>? nodeData))
                        {
                            nodeData = new List<SyntaxNode>();
                            newData.Add(owner, nodeData);
                        }
                        nodeData.Add(syntax);
                    }
                }
                return new Dealer(newData.ToImmutableDictionary(_e => _e.Key, _e => _e.Value.ToImmutableArray()));
            }

        }

        private Bundle(BodyNode _owner, ImmutableArray<SyntaxNode> _previousData, int _index)
        {
            Owner = _owner;
            HasPreviousData = !_previousData.IsDefault;
            PreviousData = _previousData;
            m_index = _index;
        }

        private readonly int m_index;
        internal BodyNode Owner { get; }
        internal bool HasPreviousData { get; }
        internal ImmutableArray<SyntaxNode> PreviousData { get; }

        internal TNode Mark<TNode>(TNode _node) where TNode : SyntaxNode
            => _node.WithAdditionalAnnotations(CreateAnnotation(m_index));

    }

}
