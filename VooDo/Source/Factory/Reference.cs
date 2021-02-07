﻿

using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;

using VooDo.Factory.Syntax;
using VooDo.Transformation;
using VooDo.Utils;

namespace VooDo.Factory
{

    public sealed class Reference : IEquatable<Reference>
    {

        public static Reference RuntimeReference { get; }
            = Reference.FromAssembly(Assembly.GetExecutingAssembly(), Identifiers.referenceAlias);

        private sealed class MetadataEqualityComparerImpl : IEqualityComparer<Reference>
        {
            public bool Equals(Reference? _x, Reference? _y) => AreSameMetadata(_x, _y);
            public int GetHashCode(Reference _obj) => _obj.GetHashCode();
        }

        public static IEqualityComparer<Reference> MetadataEqualityComparer { get; } = new MetadataEqualityComparerImpl();

        public static IEnumerable<Reference> GetSystemReferences()
        {
            HashSet<string> paths = new()
            {
                typeof(object).Assembly.Location
            };
            foreach (string path in paths)
            {
                Reference? reference = null;
                try
                {
                    reference = FromFile(path);
                }
                catch (Exception error)
                {
                    Console.WriteLine(error);
                }
                if (reference is not null)
                {
                    yield return reference;
                }
            }
        }

        public static Reference FromStream(Stream _stream, params Identifier[] _aliases)
            => FromStream(_stream, _aliases);

        public static Reference FromFile(string _path, params Identifier[] _aliases)
            => FromFile(_path, _aliases);

        public static Reference FromImage(IEnumerable<byte> _image, params Identifier[] _aliases)
            => FromImage(_image, _aliases);

        public static Reference FromAssembly(Assembly _assembly, params Identifier[] _aliases)
            => FromAssembly(_assembly, (IEnumerable<Identifier>) _aliases);

        public static Reference FromStream(Stream _stream, IEnumerable<Identifier>? _aliases = null)
        {
            return new Reference(MetadataReference.CreateFromStream(_stream), _aliases);
        }

        public static Reference FromFile(string _path, IEnumerable<Identifier>? _aliases = null)
        {
            return new Reference(MetadataReference.CreateFromFile(_path), _aliases);
        }

        public static Reference FromImage(IEnumerable<byte> _bytes, IEnumerable<Identifier>? _aliases = null)
        {
            return new Reference(MetadataReference.CreateFromImage(_bytes), _aliases);
        }

        public static Reference FromAssembly(Assembly _assembly, IEnumerable<Identifier>? _aliases = null)
        {
            return FromFile(_assembly.Location, _aliases ?? Enumerable.Empty<Identifier>());
        }

        private Reference(PortableExecutableReference _metadata, IEnumerable<Identifier>? _aliases = null)
        {
            Aliases = _aliases.EmptyIfNull().ToImmutableHashSet();
            if (Aliases.AnyNull())
            {
                throw new ArgumentException("Null alias", nameof(_aliases));
            }
            m_metadata = _metadata;
            FilePath = m_metadata.FilePath is not null ? new Uri(m_metadata.FilePath).AbsolutePath : null;
        }

        private readonly PortableExecutableReference m_metadata;

        public ImmutableHashSet<Identifier> Aliases { get; }
        public string? FilePath { get; }
        public string? DisplayName => m_metadata.Display;

        public Reference WithAliases(params Identifier[] _aliases)
            => WithAliases((IEnumerable<Identifier>) _aliases);

        public Reference WithAliases(IEnumerable<Identifier>? _aliases = null)
            => Aliases.SetEquals(_aliases.EmptyIfNull()) ? this : new Reference(m_metadata, _aliases);

        internal MetadataReference GetMetadataReference() => m_metadata.WithAliases(Aliases.Select(_a => _a.ToString()));

        public static bool AreSameMetadata(Reference _a, Reference _b)
            => _a is not null && (_a.m_metadata.Equals(_b.m_metadata) || _a.FilePath == _b.FilePath);

        public static ImmutableArray<Reference> Merge(IEnumerable<Reference> _references)
            => _references
                .GroupBy(_r => _r, MetadataEqualityComparer)
                .Select(_r => _r.Key.WithAliases(_r.SelectMany(_v => _v.Aliases)))
                .ToImmutableArray();

        public override bool Equals(object? _obj) => Equals(_obj as Reference);
        public bool Equals(Reference? _other) => _other is not null && m_metadata.Equals(_other.m_metadata) && Aliases.SetEquals(_other.Aliases);
        public override int GetHashCode() => FilePath?.GetHashCode() ?? m_metadata.GetHashCode();
        public static bool operator ==(Reference? _left, Reference? _right) => Identity.AreEqual(_left, _right);
        public static bool operator !=(Reference? _left, Reference? _right) => !(_left == _right);
        public override string ToString()
            => $"{{{nameof(Reference)}: {DisplayName ?? FilePath ?? m_metadata.GetMetadataId().ToString()}}}";

    }

}
