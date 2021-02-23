using System;
using System.Collections.Generic;
using System.Linq;

using VooDo.Compiling;
using VooDo.Problems;

namespace VooDo.AST
{

    public abstract record Node
    {

        public virtual bool Equals(Node? _other) => true;
        public override int GetHashCode() => 0;

        internal Node() { }

        protected Node(Node _copy)
        {
            Root = null;
            Compilation = null;
            m_parent = null;
            Origin = _copy.Origin;
            UserData = _copy.UserData;
        }

        public enum ERootKind
        {
            Free, Script, HookInitializer
        }

        public Node? Root { get; private set; }
        public Compilation? Compilation { get; private init; }

        public Origin Origin { get; init; } = Origin.Unknown;
        public object? UserData { get; init; }

        private Node? m_parent;
        public virtual Node? Parent => m_parent;

        public virtual IEnumerable<Node> Children
            => Enumerable.Empty<Node>();

        public IEnumerable<Problem> GetSyntaxProblems()
            => GetSelfSyntaxProblems().Concat(Children.SelectMany(_c => _c.GetSyntaxProblems()));

        protected virtual IEnumerable<Problem> GetSelfSyntaxProblems()
            => Enumerable.Empty<Problem>();

        protected internal abstract Node ReplaceNodes(Func<Node?, Node?> _map);

        internal Node SetAsRootInternal(Compilation? _compilation)
        {
            Node root = this.ReplaceNonNullDescendantNodesAndSelf(_n => _n with
            {
                Compilation = _compilation
            })!;
            foreach ((Node node, Node? parent) in root.DescendantNodesAndSelfWithParents())
            {
                node.m_parent = parent;
                node.Root = root;
            }
            return root;
        }

    }

    public abstract record BodyNodeOrIdentifier : Node
    {


    }

    public abstract record BodyNode : BodyNodeOrIdentifier
    {

    }

}
