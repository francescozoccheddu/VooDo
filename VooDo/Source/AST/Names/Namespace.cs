using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST.Directives;
using VooDo.Compiling.Emission;
using VooDo.Parsing;
using VooDo.Problems;
using VooDo.Utils;

namespace VooDo.AST.Names
{

    public sealed record Namespace : BodyNode
    {

        #region Creation

        public static Namespace Parse(string _namespace)
            => Parser.Namespace(_namespace);

        #endregion

        #region Conversion

        public static implicit operator Namespace(string _namespace) => Parse(_namespace);
        public static implicit operator Namespace(Identifier _path) => new Namespace(new[] { _path });
        public static implicit operator Namespace(Identifier[] _path) => new Namespace(_path);
        public static implicit operator Namespace(List<Identifier> _path) => new Namespace(_path.ToImmutableArray());
        public static implicit operator Namespace(ImmutableArray<Identifier> _path) => new Namespace(_path);
        public static implicit operator string(Namespace _namespace) => _namespace.ToString();

        #endregion

        #region Additional contructors

        public Namespace(params Identifier[] _path) : this(null, _path.ToImmutableArray()) { }
        public Namespace(Identifier? _alias, params Identifier[] _path) : this(_alias, _path.ToImmutableArray()) { }
        public Namespace(IEnumerable<Identifier> _path) : this(null, _path.ToImmutableArray()) { }
        public Namespace(Identifier? _alias, IEnumerable<Identifier> _path) : this(_alias, _path.ToImmutableArray()) { }
        public Namespace(ImmutableArray<Identifier> _path) : this(null, _path) { }

        #endregion

        #region Members

        public Namespace(Identifier? _alias, ImmutableArray<Identifier> _path)
        {
            Alias = _alias;
            Path = _path;
        }

        public Identifier? Alias { get; init; }

        private ImmutableArray<Identifier> m_path;
        public ImmutableArray<Identifier> Path
        {
            get => m_path;
            init
            {
                if (value.IsDefaultOrEmpty)
                {
                    throw new SyntaxError(this, "Namespace path cannot be empty").AsThrowable();
                }
                m_path = value;
            }

        }
        public bool IsAliasQualified => Alias is not null;

        #endregion

        #region Overrides

        public override UsingNamespaceDirective? Parent => (UsingNamespaceDirective?) base.Parent;

        public override Namespace ReplaceNodes(Func<Node?, Node?> _map)
        {
            Identifier? newAlias = (Identifier?) _map(Alias);
            ImmutableArray<Identifier> newPath = Path.Map(_map).NonNull();
            if (ReferenceEquals(newAlias, Alias) && newPath == Path)
            {
                return this;
            }
            else
            {
                return this with
                {
                    Alias = newAlias,
                    Path = newPath
                };
            }
        }

        internal override NameSyntax EmitNode(Scope _scope, Tagger _tagger)
        {
            IdentifierNameSyntax[] path = Path.Select(_i => SyntaxFactory.IdentifierName(_i.EmitToken(_tagger)).Own(_tagger, _i)).ToArray();
            NameSyntax type = path[0];
            if (IsAliasQualified)
            {
                type = SyntaxFactory.AliasQualifiedName(
                    SyntaxFactory.IdentifierName(Alias!.EmitToken(_tagger)),
                    (SimpleNameSyntax) type);
            }
            foreach (SimpleType name in Path.Skip(1))
            {
                type = SyntaxFactory.QualifiedName(type, (SimpleNameSyntax) name.EmitNode(_scope, _tagger));
            }
            return type.Own(_tagger, this);
        }

        public override IEnumerable<Node> Children => (IsAliasQualified ? new Node[] { Alias! } : Enumerable.Empty<Node>()).Concat(Path);
        public override string ToString() => (IsAliasQualified ? $"{Alias}::" : "") + string.Join('.', Path);

        #endregion

    }
}
