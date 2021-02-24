
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

using VooDo.AST.Names;
using VooDo.Problems;
using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public abstract record TupleExpressionBase<TElement> : AssignableExpression, IReadOnlyList<TElement> where TElement : TupleExpressionBase<TElement>.ElementBase
    {


        public abstract record ElementBase : Node
        {

        }



        private ImmutableArray<TElement> m_elements;
        private ImmutableArray<TElement> m_Elements
        {
            get => m_elements;
            init
            {
                if (value.Length < 2)
                {
                    throw new SyntaxError(this, "A tuple must have at least two elements").AsThrowable();
                }
                m_elements = value;
            }
        }

        private protected TupleExpressionBase(ImmutableArray<TElement> _elements)
        {
            m_Elements = _elements;
        }



        protected internal sealed override Node ReplaceNodes(Func<Node?, Node?> _map)
        {
            ImmutableArray<TElement> newSizes = m_Elements.Map(_map).NonNull();
            if (newSizes == m_Elements)
            {
                return this;
            }
            else
            {
                return this with
                {
                    m_Elements = newSizes
                };
            }
        }

        public TElement this[int _index] => ((IReadOnlyList<TElement>) m_Elements)[_index];
        public int Count => ((IReadOnlyCollection<TElement>) m_Elements).Count;
        public IEnumerator<TElement> GetEnumerator() => ((IEnumerable<TElement>) m_Elements).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) m_Elements).GetEnumerator();

        protected sealed override EPrecedence m_Precedence => EPrecedence.Primary;

        public override IEnumerable<Node> Children => m_Elements;
        public override string ToString() => $"({string.Join(", ", m_Elements)})";


    }

    public sealed record TupleExpression : TupleExpressionBase<TupleExpression.Element>
    {

        public TupleExpression(ImmutableArray<Element> _elements) : base(_elements) { }

        public sealed record Element(Identifier? Name, Expression Expression) : ElementBase
        {

            public bool IsNamed => Name is not null;

            protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
            {
                Identifier? newName = (Identifier?) _map(Name);
                Expression newExpression = (Expression) _map(Expression).NonNull();
                if (ReferenceEquals(newName, Name) && ReferenceEquals(newExpression, Expression))
                {
                    return this;
                }
                else
                {
                    return this with
                    {
                        Name = newName,
                        Expression = newExpression
                    };
                }
            }

            public override IEnumerable<Node> Children => IsNamed ? new Node[] { Name!, Expression } : new Node[] { Expression };
            public override string ToString() => IsNamed ? $"{Name}: {Expression}" : $"{Expression}";

        }

    }

    public sealed record TupleDeclarationExpression : TupleExpressionBase<TupleDeclarationExpression.Element>
    {

        public TupleDeclarationExpression(ImmutableArray<Element> _elements) : base(_elements) { }

        public sealed record Element(ComplexTypeOrVar Type, IdentifierOrDiscard Name) : ElementBase
        {

            protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
            {
                ComplexTypeOrVar newType = (ComplexTypeOrVar) _map(Type).NonNull();
                IdentifierOrDiscard newName = (IdentifierOrDiscard) _map(Name).NonNull();
                if (ReferenceEquals(newType, Type) && ReferenceEquals(newName, Name))
                {
                    return this;
                }
                else
                {
                    return this with
                    {
                        Type = newType,
                        Name = newName
                    };
                }
            }


            public override IEnumerable<Node> Children => new Node[] { Type, Name };
            public override string ToString() => $"{Type} {Name}";

        }

    }

}
