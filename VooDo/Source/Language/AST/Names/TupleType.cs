using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

using VooDo.Compilation;
using VooDo.Language.Linking;
using VooDo.Utils;

namespace VooDo.Language.AST.Names
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

        public static new TupleType FromSyntax(TypeSyntax _syntax, bool _ignoreUnbound = false)
            => FromSyntax((TupleTypeSyntax) Unwrap(_syntax, out bool nullable, out ImmutableArray<int> ranks), _ignoreUnbound) with
            {
                IsNullable = nullable,
                Ranks = ranks
            };

        public static TupleType FromSyntax(TupleTypeSyntax _syntax, bool _ignoreUnbound = false)
            => new TupleType(_syntax.Elements.Select(_e => new Element(
                FromSyntax(_e.Type, _ignoreUnbound),
                Identifier.FromSyntax(_e.Identifier))));

        public static new TupleType Parse(string _type, bool _ignoreUnbound = false)
            => FromSyntax(SyntaxFactory.ParseTypeName(_type), _ignoreUnbound);

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
            if (_type.IsAssignableTo(typeof(ITuple)) && s_tupleTypes.Contains(_type.GetGenericTypeDefinition()))
            {
                return new TupleType(_type.GenericTypeArguments.Select(_t => new Element(ComplexType.FromType(_t, _ignoreUnbound))));
            }
            else
            {
                throw new ArgumentException("Not a tuple type", nameof(_type));
            }
        }

        public static new TupleType FromType<TType>() where TType : ITuple
            => FromType(typeof(TType));

        #endregion

        #region Conversion

        public static implicit operator TupleType(string _type) => Parse(_type);
        public static implicit operator TupleType(Type _type) => FromType(_type);
        public static implicit operator string(TupleType _tupleType) => _tupleType.ToString();

        #endregion

        #region Nested types

        public sealed record Element(ComplexType Type, Identifier? Name = null) : Node
        {

            public static implicit operator Element(string _type) => Parse(_type);
            public static implicit operator Element(Type _type) => FromType(_type);
            public static implicit operator Element(Identifier _name) => new Element(new SimpleType(_name));
            public static implicit operator Element(SimpleType _simpleType) => new Element(_simpleType);
            public static implicit operator Element(ComplexType _complexType) => new Element(_complexType);

            public bool IsNamed => Name is not null;

            internal override SyntaxNode EmitNode(Scope _scope, Marker _marker)
                => SyntaxFactory.TupleElement(
                    Type.EmitNode(_scope, _marker),
                    IsNamed
                    ? Name!.EmitToken(_marker)
                    : SyntaxFactory.Token(SyntaxKind.None))
                .Own(_marker, this);
            public override IEnumerable<NodeOrIdentifier> Children => IsNamed ? new NodeOrIdentifier[] { Type, Name! } : new NodeOrIdentifier[] { Type };
            public override string ToString() => IsNamed ? $"{Type} {Name}" : $"{Type}";

        }

        #endregion

        #region Delegating constructors

        public TupleType(params Element[] _elements) : this(_elements.ToImmutableArray()) { }
        public TupleType(IEnumerable<ComplexType> _types) : this(_types.Select(_t => new Element(_t)).ToImmutableArray()) { }
        public TupleType(IEnumerable<Element> _elements) : this(_elements.ToImmutableArray()) { }

        #endregion

        #region Members

        private readonly ImmutableArray<Element> m_elements;

        public TupleType(ImmutableArray<Element> _elements)
        {
            if (_elements.Length < 2)
            {
                throw new ArgumentException("A tuple must have at least two elements", nameof(_elements));
            }
            m_elements = _elements;
        }

        #endregion

        #region Overrides

        public int Count => ((IReadOnlyCollection<Element>) m_elements).Count;
        public Element this[int _index] => ((IReadOnlyList<Element>) m_elements)[_index];
        public IEnumerator<Element> GetEnumerator() => ((IEnumerable<Element>) m_elements).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) m_elements).GetEnumerator();
        internal override TypeSyntax EmitNonArrayNonNullableType(Scope _scope, Marker _marker)
            => SyntaxFactory.TupleType(this.Select(_e => _e.EmitNode(_scope, _marker)).ToSeparatedList()).Own(_marker, this);
        public override IEnumerable<Element> Children => m_elements;
        public override string ToString() => $"({string.Join(',', m_elements)})" + base.ToString();

        #endregion

    }

}
