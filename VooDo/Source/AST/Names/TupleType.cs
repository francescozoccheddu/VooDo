using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Compiling.Emission;
using VooDo.Parsing;
using VooDo.Problems;
using VooDo.Utils;

namespace VooDo.AST.Names
{

    public sealed record TupleType : ComplexType, IReadOnlyList<TupleType.Element>
    {

        #region Creation

        private static readonly ImmutableHashSet<Type> s_tupleTypes = new Type[] {
            typeof(ValueTuple<>), typeof(ValueTuple<,>),
            typeof(ValueTuple<,,>), typeof(ValueTuple<,,,>),
            typeof(ValueTuple<,,,,>), typeof(ValueTuple<,,,,,>),
            typeof(ValueTuple<,,,,,,>), typeof(ValueTuple<,,,,,,,>)
        }.ToImmutableHashSet();

        public static new TupleType Parse(string _type)
            => Parser.TupleType(_type);

        public static TupleType FromTypes(IEnumerable<Type> _types, bool _ignoreUnbound = false)
            => new TupleType(_types.Select(_t => new Element(ComplexType.FromType(_t, _ignoreUnbound))));

        public static new TupleType FromType(Type _type, bool _ignoreUnbound = false)
        {
            if (_type.IsGenericTypeDefinition)
            {
                throw new ArgumentException("Unbound tuple type", nameof(_type));
            }
            if (_type.IsGenericParameter)
            {
                throw new ArgumentException("Generic parameter type", nameof(_type));
            }
            if (_type.IsPointer)
            {
                throw new ArgumentException("Pointer type", nameof(_type));
            }
            if (_type.IsByRef)
            {
                throw new ArgumentException("Ref type", nameof(_type));
            }
            if (IsTuple(_type))
            {
                return new TupleType(_type.GenericTypeArguments.Select(_t => new Element(ComplexType.FromType(_t, _ignoreUnbound))));
            }
            else
            {
                throw new ArgumentException("Not a tuple type", nameof(_type));
            }
        }

        internal static bool IsTuple(Type _type)
            => _type.IsGenericType && s_tupleTypes.Contains(_type.GetGenericTypeDefinition());

        public static new TupleType FromType<TType>()
            => FromType(typeof(TType));

        #endregion

        #region Conversion

        public static implicit operator TupleType(string _type) => Parse(_type);
        public static implicit operator TupleType(Type _type) => FromType(_type);
        public static implicit operator string(TupleType _tupleType) => _tupleType.ToString();

        #endregion

        #region Nested types

        public sealed record Element(ComplexType Type, Identifier? Name = null) : BodyNode
        {

            public static implicit operator Element(string _type) => Parse(_type);
            public static implicit operator Element(Type _type) => FromType(_type);
            public static implicit operator Element(Identifier _name) => new Element(new SimpleType(_name));
            public static implicit operator Element(SimpleType _simpleType) => new Element(_simpleType);
            public static implicit operator Element(ComplexType _complexType) => new Element(_complexType);

            public bool IsNamed => Name is not null;

            protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
            {
                ComplexType newType = (ComplexType) _map(Type).NonNull();
                Identifier? newName = (Identifier?) _map(Name).NonNull();
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
            internal override SyntaxNode EmitNode(Scope _scope, Tagger _tagger)
                => SyntaxFactory.TupleElement(
                    (TypeSyntax) Type.EmitNode(_scope, _tagger),
                    IsNamed
                    ? Name!.EmitToken(_tagger)
                    : SyntaxFactory.Token(SyntaxKind.None))
                .Own(_tagger, this);
            public override IEnumerable<Node> Children => IsNamed ? new Node[] { Type, Name! } : new Node[] { Type };
            public override string ToString() => IsNamed ? $"{Type} {Name}" : $"{Type}";

        }

        #endregion

        #region Delegating constructors

        public TupleType(params Element[] _elements) : this(_elements.ToImmutableArray()) { }
        public TupleType(IEnumerable<ComplexType> _types) : this(_types.Select(_t => new Element(_t)).ToImmutableArray()) { }
        public TupleType(IEnumerable<Element> _elements) : this(_elements.ToImmutableArray()) { }

        #endregion

        #region Members

        private ImmutableArray<Element> m_elements;
        private ImmutableArray<Element> m_Elements
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
        public TupleType(ImmutableArray<Element> _elements)
        {
            m_Elements = _elements;
        }

        #endregion

        #region Overrides

        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
        {
            ImmutableArray<Element> newElements = m_Elements.Map(_map).NonNull();
            TupleType newThis = (TupleType) base.ReplaceNodes(_map);
            if (newElements == m_Elements)
            {
                return newThis;
            }
            else
            {
                return newThis with
                {
                    m_Elements = newElements
                };
            }
        }

        public int Count => ((IReadOnlyCollection<Element>) m_Elements).Count;
        public Element this[int _index] => ((IReadOnlyList<Element>) m_Elements)[_index];
        public IEnumerator<Element> GetEnumerator() => ((IEnumerable<Element>) m_Elements).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) m_Elements).GetEnumerator();
        private protected override TypeSyntax EmitNonArrayNonNullableType(Scope _scope, Tagger _tagger)
            => SyntaxFactory.TupleType(this.Select(_e => _e.EmitNode(_scope, _tagger)).ToSeparatedList()).Own(_tagger, this);
        public override IEnumerable<Node> Children => m_Elements;
        public override string ToString() => $"({string.Join(",", m_Elements)})" + base.ToString();

        #endregion

    }

}
