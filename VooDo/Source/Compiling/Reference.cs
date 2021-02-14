

using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using VooDo.AST.Names;
using VooDo.Utils;

namespace VooDo.Compiling
{

    public sealed record Reference
    {

        public static Reference RuntimeReference { get; }
            = FromAssembly(Assembly.GetExecutingAssembly(), CompilationConstants.runtimeReferenceAlias);

        private sealed class MetadataEqualityComparerImpl : IEqualityComparer<Reference>
        {
            public bool Equals(Reference? _x, Reference? _y) => AreSameMetadata(_x, _y);
            public int GetHashCode(Reference _obj) => _obj.GetHashCode();
        }

        public static IEqualityComparer<Reference> MetadataEqualityComparer { get; } = new MetadataEqualityComparerImpl();

        public static ImmutableArray<Reference> GetSystemReferences()
        {
            string directory = RuntimeEnvironment.GetRuntimeDirectory();
            string[] names =
            {
                "System.Runtime.dll",
                "mscorlib.dll",
                "System.dll",
                typeof(object).Assembly.Location,
                typeof(int).Assembly.Location,
            };
            return names.Select(_n => FromFile(Path.Combine(directory, _n))).ToImmutableArray();
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

        public ImmutableHashSet<Identifier> Aliases { get; init; }
        public string? FilePath { get; }
        public string? DisplayName => m_metadata.Display;

        internal MetadataReference GetMetadataReference() => m_metadata.WithAliases(Aliases.Select(_a => _a.ToString()));

        public static bool AreSameMetadata(Reference _a, Reference _b)
            => _a is not null && (_a.m_metadata.Equals(_b.m_metadata) || _a.FilePath == _b.FilePath);

        public static ImmutableArray<Reference> Merge(IEnumerable<Reference> _references)
            => _references
                .GroupBy(_r => _r, MetadataEqualityComparer)
                .Select(_r => _r.Key with { Aliases = _r.SelectMany(_v => _v.Aliases).ToImmutableHashSet() })
                .ToImmutableArray();

    }

}
