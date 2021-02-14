using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST.Names;
using VooDo.Compiling;
using VooDo.Compiling.Emission;
using VooDo.Problems;
using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public abstract record TupleExpressionBase<TElement> : AssignableExpression, IReadOnlyList<TElement> where TElement : TupleExpressionBase<TElement>.ElementBase
    {

        #region Nested types

        public abstract record ElementBase : BodyNode
        {

            public abstract override ElementBase ReplaceNodes(Func<Node?, Node?> _map);
            internal abstract override ArgumentSyntax EmitNode(Scope _scope, Tagger _tagger);

        }

        #endregion

        #region Members

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

        #endregion

        #region Overrides

        public sealed override TupleExpressionBase<TElement> ReplaceNodes(Func<Node?, Node?> _map)
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
        internal sealed override TupleExpressionSyntax EmitNode(Scope _scope, Tagger _tagger)
            => SyntaxFactory.TupleExpression(this.Select(_e => _e.EmitNode(_scope, _tagger)).ToSeparatedList())
            .Own(_tagger, this);
        public override IEnumerable<TElement> Children => m_Elements;
        public override string ToString() => $"({string.Join(", ", m_Elements)})";

        #endregion

    }

    public sealed record TupleExpression : TupleExpressionBase<TupleExpression.Element>
    {

        public TupleExpression(ImmutableArray<Element> _elements) : base(_elements) { }

        public sealed record Element(Identifier? Name, Expression Expression) : ElementBase
        {

            public bool IsNamed => Name is not null;

            public override Element ReplaceNodes(Func<Node?, Node?> _map)
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

            internal override ArgumentSyntax EmitNode(Scope _scope, Tagger _tagger)
                => SyntaxFactory.Argument(Expression.EmitNode(_scope, _tagger))
                .WithNameColon(IsNamed
                    ? SyntaxFactory.NameColon(SyntaxFactory.IdentifierName(Name!.EmitToken(_tagger)).Own(_tagger, Name))
                    : null)
                .Own(_tagger, this);
            public override IEnumerable<Node> Children => IsNamed ? new Node[] { Name!, Expression } : new Node[] { Expression };
            public override string ToString() => IsNamed ? $"{Name}: {Expression}" : $"{Expression}";

        }

    }

    public sealed record TupleDeclarationExpression : TupleExpressionBase<TupleDeclarationExpression.Element>
    {

        public TupleDeclarationExpression(ImmutableArray<Element> _elements) : base(_elements) { }

        public sealed record Element(ComplexTypeOrVar Type, IdentifierOrDiscard Name) : ElementBase
        {

            public override Element ReplaceNodes(Func<Node?, Node?> _map)
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

            internal override ArgumentSyntax EmitNode(Scope _scope, Tagger _tagger)
            {
                if (!Name.IsDiscard)
                {
                    _scope.AddLocal(Name.Identifier!);
                }
                return SyntaxFactory.Argument(
                            SyntaxFactory.DeclarationExpression(
                                Type.EmitNode(_scope, _tagger),
                                Name.EmitNode(_scope, _tagger).Own(_tagger, Name)))
                            .Own(_tagger, this);
            }

            public override IEnumerable<Node> Children => new Node[] { Type, Name };
            public override string ToString() => $"{Type} {Name}";

        }

    }

}
