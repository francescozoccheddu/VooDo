using Microsoft.CodeAnalysis.CSharp;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

using VooDo.AST;
using VooDo.AST.Names;
using VooDo.Compiling;

namespace VooDo.Utils
{

    public static class TypeAliasResolver
    {

        private static readonly ImmutableHashSet<string> s_predefinedTypeNames =
            new SyntaxKind[] {
                SyntaxKind.BoolKeyword,
                SyntaxKind.CharKeyword,
                SyntaxKind.StringKeyword,
                SyntaxKind.ByteKeyword,
                SyntaxKind.SByteKeyword,
                SyntaxKind.ShortKeyword,
                SyntaxKind.UShortKeyword,
                SyntaxKind.IntKeyword,
                SyntaxKind.UIntKeyword,
                SyntaxKind.LongKeyword,
                SyntaxKind.ULongKeyword,
                SyntaxKind.DecimalKeyword,
                SyntaxKind.FloatKeyword,
                SyntaxKind.DoubleKeyword,
                SyntaxKind.ObjectKeyword
            }
            .Select(_k => SyntaxFactory.Token(_k).ValueText)
            .ToImmutableHashSet();

        public static ComplexType Resolve(Type _type, ImmutableArray<Reference> _references)
            => Resolve(ComplexType.FromType(_type), _references);

        public static ComplexType Resolve(ComplexType _type, ImmutableArray<Reference> _references)
            => _type.SetAsRoot().ReplaceNonNullDescendantNodesAndSelf(_n => _n is QualifiedType type
                    ? ResolveSingleNode(type, _references)
                    : _n)!;

        private static QualifiedType ResolveSingleNode(QualifiedType _type, ImmutableArray<Reference> _references)
        {
            if (_type.Alias is null)
            {
                string typename = GetQualifiedTypeName(_type);
                Type? type = Type.GetType(typename);
                if (type is not null)
                {
                    Assembly assembly = type.Assembly;
                    string path = NormalizeFilePath.Normalize(assembly.Location);
                    string? alias = _references
                        .FirstOrDefault(_r => _r.Assembly == assembly || _r.FilePath == path)?
                        .Aliases
                        .FirstOrDefault()!;
                    if (alias is not null)
                    {
                        return _type with { Alias = alias };
                    }
                    else if (!_type.IsSimple || !s_predefinedTypeNames.Contains(_type.Path[0].Name))
                    {
                        return _type with { Alias = "global" };
                    }
                }
            }
            return _type;
        }

        private static string GetQualifiedTypeName(QualifiedType _type)
        {
            string name = string.Join("+", _type.Path.Select(GetSimpleTypeName));
            if (_type.IsNullable)
            {
                name += "?";
            }
            name += string.Concat(_type.Ranks);
            return name;
        }

        private static string GetSimpleTypeName(SimpleType _type)
        {
            if (_type.TypeArguments.IsEmpty)
            {
                return $"{_type}";
            }
            else
            {
                return $"{_type.Name}`{_type.TypeArguments.Length}";
            }
        }

    }

}
