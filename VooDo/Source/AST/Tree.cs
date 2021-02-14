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

        private static IEnumerable<Node> VisitDepthFirstPostorder(Node _node, Predicate<Node> _shouldVisitChildren)
        {
            if (!_shouldVisitChildren(_node))
            {
                yield return _node;
                yield break;
            }
            Stack<(IEnumerator<Node> children, Node node)> stack = new();
            stack.Push((_node.Children.GetEnumerator(), _node));
            while (stack.Count > 0)
            {
                (IEnumerator<Node> children, Node node) = stack.Peek();
                if (children.MoveNext())
                {
                    Node child = children.Current;
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

        private static IEnumerable<Node> VisitDepthFirstPreorder(Node _node, Predicate<Node> _shouldVisitChildren)
        {
            Stack<Node> stack = new();
            stack.Push(_node);
            while (stack.Count > 0)
            {
                Node node = stack.Pop();
                yield return node;
                if (_shouldVisitChildren(node))
                {
                    foreach (Node child in node.Children.Reverse())
                    {
                        stack.Push(child);
                    }
                }
            }
        }

        private static IEnumerable<Node> VisitBreadthFirst(Node _node, Predicate<Node> _shouldVisitChildren)
        {
            Queue<Node> queue = new();
            queue.Enqueue(_node);
            while (queue.Count > 0)
            {
                Node node = queue.Dequeue();
                yield return node;
                if (_shouldVisitChildren(node))
                {
                    foreach (Node child in node.Children)
                    {
                        queue.Enqueue(child);
                    }
                }
            }
        }

        public static IEnumerable<Node> DescendantNodes(this Node _node, ETraversal _traversal = ETraversal.PostDepthFirst)
            => DescendantNodes(_node, _ => true, _traversal);

        public static IEnumerable<Node> DescendantNodes(this Node _node, Predicate<Node> _shouldVisitChildren, ETraversal _traversal = ETraversal.PostDepthFirst)
            => _traversal switch
            {
                ETraversal.BreadthFirst => VisitBreadthFirst(_node, _shouldVisitChildren),
                ETraversal.PreDepthFirst => VisitDepthFirstPreorder(_node, _shouldVisitChildren),
                ETraversal.PostDepthFirst => VisitDepthFirstPostorder(_node, _shouldVisitChildren),
                _ => throw new InvalidOperationException(),
            };

        public static IEnumerable<Node> DescendantNodesAndSelf(this Node _node, ETraversal _traversal = ETraversal.PostDepthFirst)
            => DescendantNodesAndSelf(_node, _ => true, _traversal);

        public static IEnumerable<Node> DescendantNodesAndSelf(this Node _node, Predicate<Node> _shouldVisitChildren, ETraversal _traversal = ETraversal.PostDepthFirst)
            => _traversal switch
            {
                ETraversal.BreadthFirst or ETraversal.PreDepthFirst => DescendantNodes(_node, _shouldVisitChildren, _traversal).Skip(1),
                ETraversal.PostDepthFirst => DescendantNodes(_node, _shouldVisitChildren, _traversal).SkipLast(1),
                _ => throw new InvalidOperationException(),
            };

        public readonly struct ReplaceInfo
        {
            public ReplaceInfo(Node? _parent, Node? _original, Node? _replaced)
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
            if (_node is not null && _shouldVisitChildren(_node) && _node.Children.Any())
            {
                return _node.ReplaceNodes(_c => ReplaceNodeRecursive(_c, _parent, _map, _shouldVisitChildren));
            }
            else
            {
                return _map(new ReplaceInfo(_parent, _node, _node));
            }
        }

        public static TNode? ReplaceDescendantNodes<TNode>(this TNode _node, Func<Node?, Node?> _map) where TNode : Node
            => ReplaceDescendantNodes(_node, _map, _ => true);

        public static TNode? ReplaceDescendantNodes<TNode>(this TNode _node, Func<Node?, Node?> _map, Predicate<Node> _shouldVisitChildren) where TNode : Node
            => ReplaceDescendantNodes(_node, _i => _map(_i.Replaced), _shouldVisitChildren);

        public static TNode? ReplaceDescendantNodes<TNode>(this TNode _node, Func<ReplaceInfo, Node?> _map) where TNode : Node
            => ReplaceDescendantNodes(_node, _map, _ => true);

        public static TNode? ReplaceDescendantNodes<TNode>(this TNode _node, Func<ReplaceInfo, Node?> _map, Predicate<Node> _shouldVisitChildren) where TNode : Node
            => (TNode?) ReplaceNodeRecursive(_node, null, _map, _shouldVisitChildren);

        public static TNode SetAsRoot<TNode>(this TNode _node) where TNode : Node
            => (TNode) _node.SetAsRootInternal(null);

        internal static TNode SetAsRoot<TNode>(this TNode _node, Compilation _compilation) where TNode : Node
            => (TNode) _node.SetAsRootInternal(_compilation);

    }

}
