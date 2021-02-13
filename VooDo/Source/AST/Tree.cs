using System;
using System.Collections.Generic;
using System.Linq;

namespace VooDo.AST
{

    public static class Tree
    {

        public enum ETraversal
        {
            BreadthFirst, PreDepthFirst, PostDepthFirst
        }

        private static IEnumerable<NodeOrIdentifier> VisitDepthFirstPostorder(NodeOrIdentifier _node, Predicate<NodeOrIdentifier> _shouldVisitChildren)
        {
            if (!_shouldVisitChildren(_node))
            {
                yield return _node;
                yield break;
            }
            Stack<(IEnumerator<NodeOrIdentifier> children, NodeOrIdentifier node)> stack = new();
            stack.Push((_node.Children.GetEnumerator(), _node));
            while (stack.Count > 0)
            {
                (IEnumerator<NodeOrIdentifier> children, NodeOrIdentifier node) = stack.Peek();
                if (children.MoveNext())
                {
                    NodeOrIdentifier child = children.Current;
                    if (_shouldVisitChildren(child))
                    {
                        stack.Push((child.Children.GetEnumerator(), child));
                    }
                    else
                    {
                        yield return child;
                    }
                }
                else
                {
                    _ = stack.Pop();
                    yield return node;
                }
            }
        }

        private static IEnumerable<NodeOrIdentifier> VisitDepthFirstPreorder(NodeOrIdentifier _node, Predicate<NodeOrIdentifier> _shouldVisitChildren)
        {
            Stack<NodeOrIdentifier> stack = new();
            stack.Push(_node);
            while (stack.Count > 0)
            {
                NodeOrIdentifier node = stack.Pop();
                yield return node;
                if (_shouldVisitChildren(node))
                {
                    foreach (NodeOrIdentifier child in node.Children.Reverse())
                    {
                        stack.Push(child);
                    }
                }
            }
        }

        private static IEnumerable<NodeOrIdentifier> VisitBreadthFirst(NodeOrIdentifier _node, Predicate<NodeOrIdentifier> _shouldVisitChildren)
        {
            Queue<NodeOrIdentifier> queue = new();
            queue.Enqueue(_node);
            while (queue.Count > 0)
            {
                NodeOrIdentifier node = queue.Dequeue();
                yield return node;
                if (_shouldVisitChildren(node))
                {
                    foreach (NodeOrIdentifier child in node.Children)
                    {
                        queue.Enqueue(child);
                    }
                }
            }
        }

        public static IEnumerable<NodeOrIdentifier> DescendantNodes(this NodeOrIdentifier _node, ETraversal _traversal = ETraversal.PostDepthFirst)
            => DescendantNodes(_node, _ => true, _traversal);

        public static IEnumerable<NodeOrIdentifier> DescendantNodes(this NodeOrIdentifier _node, Predicate<NodeOrIdentifier> _shouldVisitChildren, ETraversal _traversal = ETraversal.PostDepthFirst)
            => _traversal switch
            {
                ETraversal.BreadthFirst => VisitBreadthFirst(_node, _shouldVisitChildren),
                ETraversal.PreDepthFirst => VisitDepthFirstPreorder(_node, _shouldVisitChildren),
                ETraversal.PostDepthFirst => VisitDepthFirstPostorder(_node, _shouldVisitChildren),
                _ => throw new InvalidOperationException(),
            };

        public static IEnumerable<NodeOrIdentifier> DescendantNodesAndSelf(this NodeOrIdentifier _node, ETraversal _traversal = ETraversal.PostDepthFirst)
            => DescendantNodesAndSelf(_node, _ => true, _traversal);

        public static IEnumerable<NodeOrIdentifier> DescendantNodesAndSelf(this NodeOrIdentifier _node, Predicate<NodeOrIdentifier> _shouldVisitChildren, ETraversal _traversal = ETraversal.PostDepthFirst)
            => _traversal switch
            {
                ETraversal.BreadthFirst or ETraversal.PreDepthFirst => DescendantNodes(_node, _shouldVisitChildren, _traversal).Skip(1),
                ETraversal.PostDepthFirst => DescendantNodes(_node, _shouldVisitChildren, _traversal).SkipLast(1),
                _ => throw new InvalidOperationException(),
            };

        public readonly struct ReplaceInfo
        {
            public ReplaceInfo(Node? _parent, NodeOrIdentifier? _original, NodeOrIdentifier? _replaced)
            {
                Parent = _parent;
                Original = _original;
                Replaced = _replaced;
            }

            public Node? Parent { get; }
            public NodeOrIdentifier? Original { get; }
            public NodeOrIdentifier? Replaced { get; }
        }

        private static NodeOrIdentifier? ReplaceNodeRecursive(NodeOrIdentifier? _node, Node? _parent, Func<ReplaceInfo, NodeOrIdentifier?> _map, Predicate<NodeOrIdentifier> _shouldVisitChildren)
        {
            if (_node is not null && _shouldVisitChildren(_node) && _node.Children.Any())
            {
                return _node.ReplaceNodes(_c => ReplaceNodeRecursive(_c, _parent, _map, _shouldVisitChildren));
            }
            else
            {
                return _map(new ReplaceInfo(_parent, _node, _node));
            }
        }

        public static TNodeOrIdentifier? ReplaceDescendantNodes<TNodeOrIdentifier>(this TNodeOrIdentifier _node, Func<NodeOrIdentifier?, NodeOrIdentifier?> _map) where TNodeOrIdentifier : NodeOrIdentifier
            => ReplaceDescendantNodes(_node, _map, _ => true);

        public static TNodeOrIdentifier? ReplaceDescendantNodes<TNodeOrIdentifier>(this TNodeOrIdentifier _node, Func<NodeOrIdentifier?, NodeOrIdentifier?> _map, Predicate<NodeOrIdentifier> _shouldVisitChildren) where TNodeOrIdentifier : NodeOrIdentifier
            => ReplaceDescendantNodes(_node, _i => _map(_i.Replaced), _shouldVisitChildren);


        public static TNodeOrIdentifier? ReplaceDescendantNodes<TNodeOrIdentifier>(this TNodeOrIdentifier _node, Func<ReplaceInfo, NodeOrIdentifier?> _map) where TNodeOrIdentifier : NodeOrIdentifier
            => ReplaceDescendantNodes(_node, _map, _ => true);

        public static TNodeOrIdentifier? ReplaceDescendantNodes<TNodeOrIdentifier>(this TNodeOrIdentifier _node, Func<ReplaceInfo, NodeOrIdentifier?> _map, Predicate<NodeOrIdentifier> _shouldVisitChildren) where TNodeOrIdentifier : NodeOrIdentifier
            => (TNodeOrIdentifier?) ReplaceNodeRecursive(_node, null, _map, _shouldVisitChildren);

    }

}
