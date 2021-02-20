using System;
using System.Collections.Generic;
using System.Linq;

using VooDo.AST.Expressions;
using VooDo.AST.Names;
using VooDo.AST.Statements;
using VooDo.Compiling;

namespace VooDo.AST
{

    public static class Tree
    {

        public static bool IsStatementAncestor(Node _node)
            => _node is Script or IfStatement or GlobalStatement or BlockStatement;

        public static bool IsExpressionAncestor(Node _node)
            => _node is Script or Statement or Expression or Argument or InvocationExpression.Callable or TupleExpression.Element or DeclarationStatement.Declarator;

        public static bool IsName(Node _node)
            => _node is Identifier or IdentifierOrDiscard or SimpleType or ComplexType or ComplexTypeOrVar or Namespace;

        public enum ETraversal
        {
            BreadthFirst, PreDepthFirst, PostDepthFirst
        }

        private static IEnumerable<(Node node, Node? parent)> VisitDepthFirstPostorder(Node _node, Predicate<Node> _shouldVisitChildren)
        {
            if (!_shouldVisitChildren(_node))
            {
                yield return (_node, null);
                yield break;
            }
            Stack<(IEnumerator<Node> children, (Node node, Node? parent))> stack = new();
            stack.Push((_node.Children.GetEnumerator(), (_node, null)));
            while (stack.Count > 0)
            {
                (IEnumerator<Node> children, (Node node, Node? parent)) = stack.Peek();
                if (children.MoveNext())
                {
                    Node child = children.Current;
                    if (_shouldVisitChildren(child))
                    {
                        stack.Push((child.Children.GetEnumerator(), (child, node)));
                    }
                    else
                    {
                        yield return (child, node);
                    }
                }
                else
                {
                    _ = stack.Pop();
                    yield return (node, parent);
                }
            }
        }

        private static IEnumerable<(Node node, Node? parent)> VisitDepthFirstPreorder(Node _node, Predicate<Node> _shouldVisitChildren)
        {
            Stack<(Node node, Node? parent)> stack = new();
            stack.Push((_node, null));
            while (stack.Count > 0)
            {
                (Node node, Node? parent) = stack.Pop();
                yield return (node, parent);
                if (_shouldVisitChildren(node))
                {
                    foreach (Node child in node.Children.Reverse())
                    {
                        stack.Push((child, node));
                    }
                }
            }
        }

        private static IEnumerable<(Node node, Node? parent)> VisitBreadthFirst(Node _node, Predicate<Node> _shouldVisitChildren)
        {
            Queue<(Node node, Node? parent)> queue = new();
            queue.Enqueue((_node, null));
            while (queue.Count > 0)
            {
                (Node node, Node? parent) = queue.Dequeue();
                yield return (node, parent);
                if (_shouldVisitChildren(node))
                {
                    foreach (Node child in node.Children)
                    {
                        queue.Enqueue((child, node));
                    }
                }
            }
        }

        public static IEnumerable<Node> DescendantNodesAndSelf(this Node _node, Predicate<Node> _shouldVisitChildren, ETraversal _traversal = ETraversal.PostDepthFirst)
            => DescendantNodesAndSelfWithParents(_node, _shouldVisitChildren, _traversal).Select(_e => _e.node);

        public static IEnumerable<Node> DescendantNodesAndSelf(this Node _node, ETraversal _traversal = ETraversal.PostDepthFirst)
            => DescendantNodesAndSelfWithParents(_node, _traversal).Select(_e => _e.node);

        public static IEnumerable<Node> DescendantNodes(this Node _node, Predicate<Node> _shouldVisitChildren, ETraversal _traversal = ETraversal.PostDepthFirst)
            => DescendantNodesWithParents(_node, _shouldVisitChildren, _traversal).Select(_e => _e.node);

        public static IEnumerable<Node> DescendantNodes(this Node _node, ETraversal _traversal = ETraversal.PostDepthFirst)
            => DescendantNodesWithParents(_node, _traversal).Select(_e => _e.node);

        public static IEnumerable<(Node node, Node? parent)> DescendantNodesWithParents(this Node _node, ETraversal _traversal = ETraversal.PostDepthFirst)
            => DescendantNodesWithParents(_node, _ => true, _traversal);

        public static IEnumerable<(Node node, Node? parent)> DescendantNodesWithParents(this Node _node, Predicate<Node> _shouldVisitChildren, ETraversal _traversal = ETraversal.PostDepthFirst)
            => _traversal switch
            {
                ETraversal.BreadthFirst => VisitBreadthFirst(_node, _shouldVisitChildren).Skip(1),
                ETraversal.PreDepthFirst => VisitDepthFirstPreorder(_node, _shouldVisitChildren).Skip(1),
                ETraversal.PostDepthFirst => VisitDepthFirstPostorder(_node, _shouldVisitChildren).SkipLast(1),
            };

        public static IEnumerable<(Node node, Node? parent)> DescendantNodesAndSelfWithParents(this Node _node, ETraversal _traversal = ETraversal.PostDepthFirst)
            => DescendantNodesAndSelfWithParents(_node, _ => true, _traversal);

        public static IEnumerable<(Node node, Node? parent)> DescendantNodesAndSelfWithParents(this Node _node, Predicate<Node> _shouldVisitChildren, ETraversal _traversal = ETraversal.PostDepthFirst)
            => _traversal switch
            {
                ETraversal.BreadthFirst or ETraversal.PreDepthFirst => DescendantNodesWithParents(_node, _shouldVisitChildren, _traversal),
                ETraversal.PostDepthFirst => DescendantNodesWithParents(_node, _shouldVisitChildren, _traversal),
            };

        public readonly struct ReplaceInfo
        {
            internal ReplaceInfo(Node? _parent, Node? _original, Node? _replaced)
            {
                Parent = _parent;
                Original = _original;
                Replaced = _replaced;
            }

            public Node? Parent { get; }
            public Node? Original { get; }
            public Node? Replaced { get; }
        }

        private static Node? ReplaceNodeRecursive(Node? _node, Node? _parent, Func<ReplaceInfo, Node?> _map, Predicate<Node> _shouldVisitChildren)
        {
            Node? replaced = _node;
            if (_node is not null && _shouldVisitChildren(_node) && _node.Children.Any())
            {
                replaced = _node.ReplaceNodes(_c => ReplaceNodeRecursive(_c, _node, _map, _shouldVisitChildren));
            }
            return _map(new ReplaceInfo(_parent, _node, replaced));
        }

        public static TNode? ReplaceNonNullDescendantNodesAndSelf<TNode>(this TNode _node, Func<Node, Node?> _map) where TNode : Node
            => ReplaceNonNullDescendantNodesAndSelf(_node, _map, _ => true);

        public static TNode? ReplaceNonNullDescendantNodesAndSelf<TNode>(this TNode _node, Func<Node, Node?> _map, Predicate<Node> _shouldVisitChildren) where TNode : Node
            => ReplaceDescendantNodesAndSelf(_node, _i => _i.Replaced is null ? null : _map(_i.Replaced), _shouldVisitChildren);

        public static TNode? ReplaceNonNullDescendantNodesAndSelf<TNode>(this TNode _node, Func<ReplaceInfo, Node?> _map) where TNode : Node
            => ReplaceNonNullDescendantNodesAndSelf(_node, _map, _ => true);

        public static TNode? ReplaceNonNullDescendantNodesAndSelf<TNode>(this TNode _node, Func<ReplaceInfo, Node?> _map, Predicate<Node> _shouldVisitChildren) where TNode : Node
            => ReplaceDescendantNodesAndSelf(_node, _i => _i.Replaced is null ? null : _map(_i), _shouldVisitChildren);

        public static TNode? ReplaceDescendantNodesAndSelf<TNode>(this TNode _node, Func<Node?, Node?> _map) where TNode : Node
            => ReplaceDescendantNodesAndSelf(_node, _map, _ => true);

        public static TNode? ReplaceDescendantNodesAndSelf<TNode>(this TNode _node, Func<Node?, Node?> _map, Predicate<Node> _shouldVisitChildren) where TNode : Node
            => ReplaceDescendantNodesAndSelf(_node, _i => _map(_i.Replaced), _shouldVisitChildren);

        public static TNode? ReplaceDescendantNodesAndSelf<TNode>(this TNode _node, Func<ReplaceInfo, Node?> _map) where TNode : Node
            => ReplaceDescendantNodesAndSelf(_node, _map, _ => true);

        public static TNode? ReplaceDescendantNodesAndSelf<TNode>(this TNode _node, Func<ReplaceInfo, Node?> _map, Predicate<Node> _shouldVisitChildren) where TNode : Node
            => (TNode?) ReplaceNodeRecursive(_node, null, _map, _shouldVisitChildren);

        public static TNode SetAsRoot<TNode>(this TNode _node) where TNode : Node
            => (TNode) _node.SetAsRootInternal(null);

        internal static TNode SetAsRoot<TNode>(this TNode _node, Compilation _compilation) where TNode : Node
            => (TNode) _node.SetAsRootInternal(_compilation);

    }

}
