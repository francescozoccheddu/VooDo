﻿
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Parsing;
using VooDo.Problems;
using VooDo.Utils;

namespace VooDo.AST.Names
{

    public sealed record Namespace : Node
    {

        
        public static Namespace Parse(string _namespace)
            => Parser.Namespace(_namespace);

        
        
        public static implicit operator Namespace(string _namespace) => Parse(_namespace);
        public static implicit operator Namespace(Identifier _path) => new Namespace(new[] { _path });
        public static implicit operator Namespace(Identifier[] _path) => new Namespace(_path);
        public static implicit operator Namespace(List<Identifier> _path) => new Namespace(_path.ToImmutableArray());
        public static implicit operator Namespace(ImmutableArray<Identifier> _path) => new Namespace(_path);
        public static implicit operator string(Namespace _namespace) => _namespace.ToString();

        
        
        public Namespace(params Identifier[] _path) : this(null, _path.ToImmutableArray()) { }
        public Namespace(Identifier? _alias, params Identifier[] _path) : this(_alias, _path.ToImmutableArray()) { }
        public Namespace(IEnumerable<Identifier> _path) : this(null, _path.ToImmutableArray()) { }
        public Namespace(Identifier? _alias, IEnumerable<Identifier> _path) : this(_alias, _path.ToImmutableArray()) { }
        public Namespace(ImmutableArray<Identifier> _path) : this(null, _path) { }

        
        
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

        
        

        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
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



        public override IEnumerable<Node> Children => (IsAliasQualified ? new Node[] { Alias! } : Enumerable.Empty<Node>()).Concat(Path);
        public override string ToString() => (IsAliasQualified ? $"{Alias}::" : "") + string.Join(".", Path);

        
    }
}
