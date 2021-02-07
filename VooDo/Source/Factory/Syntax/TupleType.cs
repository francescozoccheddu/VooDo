using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

using VooDo.Utils;

namespace VooDo.Factory.Syntax
{

    public sealed class TupleType : ComplexType, IReadOnlyList<TupleType.Element>, IEquatable<TupleType?>
    {

        private static readonly ImmutableHashSet<Type> s_tupleTypes = new Type[] {
            typeof(ValueTuple<>), typeof(ValueTuple<,>),
            typeof(ValueTuple<,,>), typeof(ValueTuple<,,,>),
            typeof(ValueTuple<,,,,>), typeof(ValueTuple<,,,,,>),
            typeof(ValueTuple<,,,,,,>), typeof(ValueTuple<,,,,,,,>)
        }.ToImmutableHashSet();

        public static new TupleType FromSyntax(TypeSyntax _syntax, bool _ignoreUnbound = false)
            => FromSyntax((TupleTypeSyntax) Unwrap(_syntax, out bool nullable, out ImmutableArray<int> ranks), _ignoreUnbound)
            .WithIsNullable(nullable)
            .WithRanks(ranks);

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

        public sealed class Element : IEquatable<Element?>
        {

            public static implicit operator Element(ComplexType _type) => new Element(_type);
            public static implicit operator ComplexType(Element _element) => _element.Type;

            public Element(ComplexType _type, Identifier? _name = null)
            {
                Type = _type;
                Name = _name;
            }

            public ComplexType Type { get; }
            public Identifier? Name { get; }
            public bool IsNamed => Name is not null;

            public void Deconstruct(out ComplexType _type, out Identifier? _name)
            {
                _type = Type;
                _name = Name;
            }

            public Element WithName(Identifier? _name)
                => _name == Name ? this : new Element(Type, _name);

            public Element WithType(ComplexType _type)
                => _type == Type ? this : new Element(_type, Name);

            public override bool Equals(object? _obj) => Equals(_obj as Element);
            public bool Equals(Element? _other) => _other != null && Type == _other.Type && Name == _other.Name;
            public static bool operator ==(Element? _left, Element? _right) => Identity.AreEqual(_left, _right);
            public static bool operator !=(Element? _left, Element? _right) => !(_left == _right);
            public override int GetHashCode() => Identity.CombineHash(Type, Name);
            public override string ToString() => IsNamed ? $"{Type} {Name}" : $"{Type}";

        }

        public static implicit operator TupleType(ComplexType[] _types) => new TupleType(_types);
        public static implicit operator TupleType(List<ComplexType> _types) => new TupleType(_types);
        public static implicit operator TupleType(ImmutableArray<ComplexType> _types) => new TupleType(_types);
        public static implicit operator TupleType(Element[] _types) => new TupleType(_types);
        public static implicit operator TupleType(List<Element> _types) => new TupleType(_types);
        public static implicit operator TupleType(ImmutableArray<Element> _types) => new TupleType(_types);
        public static implicit operator TupleType(string _type) => Parse(_type);

        public TupleType(params Element[] _elements) : this(_elements, false, null)
        { }

        public TupleType(IEnumerable<ComplexType> _types, params int[] _ranks) : this(_types, (IEnumerable<int>) _ranks) { }

        public TupleType(IEnumerable<ComplexType> _types, IEnumerable<int>? _ranks) : this(_types, false, _ranks) { }

        public TupleType(IEnumerable<ComplexType> _types, bool _nullable = false, IEnumerable<int>? _ranks = null) : this(_types.Select(_e => new Element(_e)), _nullable, _ranks) { }

        public TupleType(IEnumerable<Element> _elements, params int[] _ranks) : this(_elements, (IEnumerable<int>) _ranks) { }

        public TupleType(IEnumerable<Element> _elements, IEnumerable<int>? _ranks) : this(_elements, false, _ranks) { }

        public TupleType(IEnumerable<Element> _elements, bool _nullable = false, IEnumerable<int>? _ranks = null) : base(_nullable, _ranks)
        {
            m_elements = _elements.ToImmutableArray();
            if (m_elements.Length < 2)
            {
                throw new ArgumentException("Less than two elements", nameof(_elements));
            }
            Types = m_elements.Select(_e => _e.Type).ToImmutableArray();
            Names = m_elements.Select(_e => _e.Name).ToImmutableArray();
        }

        private readonly ImmutableArray<Element> m_elements;
        public ImmutableArray<ComplexType> Types { get; }
        public ImmutableArray<Identifier?> Names { get; }


        public override TupleType WithIsNullable(bool _nullable)
            => _nullable == IsNullable ? this : new TupleType(this, _nullable, Ranks);

        public override TupleType WithRanks(IEnumerable<int>? _ranks)
            => Ranks.Equals(_ranks.EmptyIfNull()) ? this : new TupleType(this, IsNullable, _ranks);

        public TupleType WithElement(Element _element, int _index)
        {
            if (_index < 0 || _index > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(_index));
            }
            if (_element == this[_index])
            {
                return this;
            }
            return new TupleType(m_elements.SetItem(_index, _element), IsNullable, Ranks);
        }

        public TupleType WithTypes(params ComplexType[] _names)
            => WithTypes((IEnumerable<ComplexType>) _names);

        public TupleType WithTypes(IEnumerable<ComplexType> _types)
        {
            ImmutableArray<ComplexType> types = _types.ToImmutableArray();
            if (types.Length != Count)
            {
                throw new ArgumentException("Wrong types count", nameof(_types));
            }
            return new TupleType(types.Zip(Names, (_t, _n) => new Element(_t, _n)), IsNullable, Ranks);
        }

        public TupleType WithNames(params Identifier?[] _names)
            => WithNames((IEnumerable<Identifier?>) _names);

        public TupleType WithNames(IEnumerable<Identifier?>? _names)
        {
            ImmutableArray<Identifier?> names = (_names ?? Enumerable.Repeat<Identifier?>(null, Count)).ToImmutableArray();
            if (names.Length != Count)
            {
                throw new ArgumentException("Wrong name count", nameof(_names));
            }
            return new TupleType(Types.Zip(names, (_t, _n) => new Element(_t, _n)), IsNullable, Ranks);
        }

        public int Count => ((IReadOnlyCollection<Element>) m_elements).Count;
        public Element this[int _index] => ((IReadOnlyList<Element>) m_elements)[_index];
        public IEnumerator<Element> GetEnumerator() => ((IEnumerable<Element>) m_elements).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) m_elements).GetEnumerator();

        public static bool operator ==(TupleType? _left, TupleType? _right) => Identity.AreEqual(_left, _right);
        public static bool operator !=(TupleType? _left, TupleType? _right) => !(_left == _right);
        public override bool Equals(object? _obj) => Equals(_obj as TupleType);
        public bool Equals(TupleType? _other) => _other is not null && base.Equals(_other) && m_elements.Equals(_other.m_elements);
        public override int GetHashCode() => Identity.CombineHashes(m_elements);
        public override string ToString() => $"({string.Join(',', m_elements)})";

    }

}
