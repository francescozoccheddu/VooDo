﻿using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Linq;

using VooDo.Compilation;

namespace VooDo.AST.Names
{

    public sealed record ComplexTypeOrVar : Node
    {

        #region Creation

        public static ComplexTypeOrVar Var { get; } = new ComplexTypeOrVar(null);

        public static ComplexTypeOrVar Parse(string _type, bool _ignoreUnbound = false)
            => FromSyntax(SyntaxFactory.ParseTypeName(_type), _ignoreUnbound);

        public static ComplexTypeOrVar FromType(Type _type, bool _ignoreUnbound = false)
            => ComplexType.FromType(_type, _ignoreUnbound);

        public static ComplexTypeOrVar FromType<TType>()
            => FromType(typeof(TType));

        public static ComplexTypeOrVar FromSyntax(TypeSyntax _type, bool _ignoreUnbound = false) =>
            _type is IdentifierNameSyntax name && name.IsVar
            ? Var
            : ComplexType.FromSyntax(_type, _ignoreUnbound);

        public static ComplexTypeOrVar FromComplexType(ComplexType _type)
            => new ComplexTypeOrVar(_type);

        #endregion

        #region Conversion

        public static implicit operator ComplexTypeOrVar(string _type) => Parse(_type);
        public static implicit operator ComplexTypeOrVar(Type _type) => FromType(_type);
        public static implicit operator ComplexTypeOrVar(SimpleType _simpleType) => new QualifiedType(_simpleType);
        public static implicit operator ComplexTypeOrVar(Identifier _identifier) => new QualifiedType(_identifier);
        public static implicit operator ComplexTypeOrVar(ComplexType _complexType) => FromComplexType(_complexType);
        public static implicit operator string(ComplexTypeOrVar _type) => _type.ToString();

        #endregion

        #region Members

        private ComplexTypeOrVar(ComplexType? _type)
        {
            Type = _type;
        }

        public ComplexType? Type { get; }
        public bool IsVar => Type is null;

        #endregion

        #region Overrides

        public override ArrayCreationExpression ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map)
        {
            ComplexType newType = (ComplexType) _map(Type).NonNull();
            ImmutableArray<Expression> newSizes = Sizes.Map(_map).NonNull();
            if (ReferenceEquals(newType, Type) && newSizes == Sizes)
            {
                return this;
            }
            else
            {
                return this with
                {
                    Type = newType,
                    Sizes = newSizes
                };
            }
        }

        internal override TypeSyntax EmitNode(Scope _scope, Marker _marker)
            => (IsVar
            ? SyntaxFactory.IdentifierName("var")
            : Type!.EmitNode(_scope, _marker))
            .Own(_marker, this);
        public override IEnumerable<ComplexType> Children => IsVar ? Enumerable.Empty<ComplexType>() : new[] { Type! };
        public override string ToString() => IsVar ? "var" : Type!.ToString();

        #endregion

    }

}
