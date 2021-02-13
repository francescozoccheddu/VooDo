using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST.Names;
using VooDo.Compilation;
using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public abstract record TupleExpressionBase<TElement> : AssignableExpression, IReadOnlyList<TElement> where TElement : TupleExpressionBase<TElement>.ElementBase
    {

        #region Nested types

        public abstract record ElementBase : Node
        {

            public abstract override ElementBase ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map);
            internal abstract override ArgumentSyntax EmitNode(Scope _scope, Marker _marker);

        }

        #endregion

        #region Members

        private readonly ImmutableArray<TElement> m_elements;

        private protected TupleExpressionBase(ImmutableArray<TElement> _elements)
        {
            if (_elements.Length < 2)
            {
                throw new ArgumentException("A tuple must have at least two elements", nameof(_elements));
            }
            m_elements = _elements;
        }

        #endregion

        #region Overrides

        protected abstract TupleExpressionBase<TElement> Create(ImmutableArray<TElement> _elements);

        public sealed override TupleExpressionBase<TElement> ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map)
        {
            ImmutableArray<TElement> newSizes = m_elements.Map(_map).NonNull();
            if (newSizes == m_elements)
            {
                return this;
            }
            else
            {
                return Create(m_elements);
            }
        }

        public TElement this[int _index] => ((IReadOnlyList<TElement>) m_elements)[_index];
        public int Count => ((IReadOnlyCollection<TElement>) m_elements).Count;
        public IEnumerator<TElement> GetEnumerator() => ((IEnumerable<TElement>) m_elements).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) m_elements).GetEnumerator();

        protected sealed override EPrecedence m_Precedence => EPrecedence.Primary;
        internal sealed override TupleExpressionSyntax EmitNode(Scope _scope, Marker _marker)
            => SyntaxFactory.TupleExpression(this.Select(_e => _e.EmitNode(_scope, _marker)).ToSeparatedList())
            .Own(_marker, this);
        public override IEnumerable<TElement> Children => m_elements;
        public override string ToString() => $"({string.Join(", ", m_elements)})";

        #endregion

    }

    public sealed record TupleExpression : TupleExpressionBase<TupleExpression.Element>
    {

        public TupleExpression(ImmutableArray<Element> _elements) : base(_elements) { }

        public sealed record Element(Identifier? Name, Expression Expression) : ElementBase
        {

            public bool IsNamed => Name is not null;

            public override Element ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map)
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

            internal override ArgumentSyntax EmitNode(Scope _scope, Marker _marker)
                => SyntaxFactory.Argument(Expression.EmitNode(_scope, _marker))
                .WithNameColon(IsNamed
                    ? SyntaxFactory.NameColon(SyntaxFactory.IdentifierName(Name!.EmitToken(_marker)).Own(_marker, Name))
                    : null)
                .Own(_marker, this);
            public override IEnumerable<NodeOrIdentifier> Children => IsNamed ? new NodeOrIdentifier[] { Name!, Expression } : new NodeOrIdentifier[] { Expression };
            public override string ToString() => IsNamed ? $"{Name}: {Expression}" : $"{Expression}";

        }

        protected override TupleExpression Create(ImmutableArray<Element> _elements) => new TupleExpression(_elements);

    }

    public sealed record TupleDeclarationExpression : TupleExpressionBase<TupleDeclarationExpression.Element>
    {

        public TupleDeclarationExpression(ImmutableArray<Element> _elements) : base(_elements) { }

        public sealed record Element(ComplexTypeOrVar Type, IdentifierOrDiscard Name) : ElementBase
        {

            public override Element ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map)
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

            internal override ArgumentSyntax EmitNode(Scope _scope, Marker _marker)
            {
                if (!Name.IsDiscard)
                {
                    _scope.AddLocal(Name.Identifier!);
                }
                return SyntaxFactory.Argument(
                            SyntaxFactory.DeclarationExpression(
                                Type.EmitNode(_scope, _marker),
                                Name.EmitNode(_scope, _marker).Own(_marker, Name)))
                            .Own(_marker, this);
            }

            public override IEnumerable<NodeOrIdentifier> Children => new NodeOrIdentifier[] { Type, Name };
            public override string ToString() => $"{Type} {Name}";

        }

        protected override TupleDeclarationExpression Create(ImmutableArray<Element> _elements) => new TupleDeclarationExpression(_elements);

    }

}
