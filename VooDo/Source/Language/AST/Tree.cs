using System.Collections.Generic;
using System.Linq;

namespace VooDo.Language.AST
{

    public static class Tree
    {

        private static IEnumerable<NodeOrIdentifier> Visit(NodeOrIdentifier _root)
        {
            Queue<NodeOrIdentifier> queue = new Queue<NodeOrIdentifier>();
            queue.Enqueue(_root);
            while (queue.Count > 0)
            {
                NodeOrIdentifier node = queue.Dequeue();
                foreach (NodeOrIdentifier child in node.Children)
                {
                    queue.Enqueue(child);
                }
                yield return node;
            }
        }

        public static IEnumerable<NodeOrIdentifier> DescendantNodes(this NodeOrIdentifier _node)
            => Visit(_node).Skip(1);

        public static IEnumerable<NodeOrIdentifier> DescendantNodesAndSelf(this NodeOrIdentifier _node)
            => Visit(_node);

    }

}
