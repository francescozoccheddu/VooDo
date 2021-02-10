
using System.Collections.Generic;

using VooDo.Factory;

namespace VooDo.Language.AST
{

    public abstract record Node
    {

        public Origin Origin { get; init; } = default;
        public abstract IEnumerable<Node> Children { get; }
        public abstract override string ToString();

    }

}
