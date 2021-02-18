

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

    public sealed record Reference : IEquatable<Reference?>
    {

        public static Reference RuntimeReference { get; }
            = FromAssembly(Assembly.GetExecutingAssembly(), CompilationConstants.runtimeReferenceAlias);

        private sealed class MetadataEqualityComparerImpl : IEqualityComparer<Reference>
        {
            public bool Equals(Reference? _x, Reference? _y) => _x is not null && _y is not null && AreSameMetadata(_x, _y);
            public int GetHashCode(Reference _obj) => _obj.DisplayName?.GetHashCode() ?? 0;
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
            => FromStream(_stream, (IEnumerable<Identifier>) _aliases);

        public static Reference FromFile(string _path, params Identifier[] _aliases)
            => FromFile(_path, (IEnumerable<Identifier>) _aliases);

        public static Reference FromImage(IEnumerable<byte> _image, params Identifier[] _aliases)
            => FromImage(_image, (IEnumerable<Identifier>) _aliases);

        public static Reference FromAssembly(Assembly _assembly, params Identifier[] _aliases)
            => FromAssembly(_assembly, (IEnumerable<Identifier>) _aliases);

        public static Reference FromStream(Stream _stream, IEnumerable<Identifier>? _aliases = null)
            => new Reference(MetadataReference.CreateFromStream(_stream), null, _aliases);

        public static Reference FromFile(string _path, IEnumerable<Identifier>? _aliases = null)
            => new Reference(MetadataReference.CreateFromFile(_path), null, _aliases);

        public static Reference FromImage(IEnumerable<byte> _bytes, IEnumerable<Identifier>? _aliases = null)
            => new Reference(MetadataReference.CreateFromImage(_bytes), null, _aliases);

        public static Reference FromAssembly(Assembly _assembly, IEnumerable<Identifier>? _aliases = null)
            => new Reference(MetadataReference.CreateFromFile(_assembly.Location), _assembly, _aliases);

        private Reference(PortableExecutableReference _metadata, Assembly? _assembly, IEnumerable<Identifier>? _aliases = null)
        {
            Aliases = _aliases.EmptyIfNull().ToImmutableHashSet();
            m_metadata = _metadata;
            string? path = m_metadata.FilePath ?? _assembly?.Location;
            FilePath = path is not null ? new Uri(path).AbsolutePath : null;
            FilePath = Uri.UnescapeDataString(FilePath);
            Assembly = _assembly;
        }

        private readonly PortableExecutableReference m_metadata;

        public ImmutableHashSet<Identifier> Aliases { get; init; }
        public string? FilePath { get; }
        public string? DisplayName => m_metadata.Display;
        public Assembly? Assembly { get; }

        internal MetadataReference GetMetadataReference() => m_metadata.WithAliases(Aliases.Select(_a => _a.ToString()));

        public static bool AreSameMetadata(Reference _a, Reference _b)
            => _a is not null && (_a.m_metadata.Equals(_b.m_metadata) || _a.FilePath == _b.FilePath || (_a.Assembly is not null && _a.Assembly.Equals(_b.Assembly)));

        public static ImmutableArray<Reference> Merge(IEnumerable<Reference> _references)
            => _references
                .GroupBy(_r => _r, MetadataEqualityComparer)
                .Select(_r => _r.Key with { Aliases = _r.SelectMany(_v => _v.Aliases).ToImmutableHashSet() })
                .ToImmutableArray();

        public bool Equals(Reference? _other) => _other is not null && AreSameMetadata(this, _other) && Aliases.SetEquals(_other.Aliases);
        public override int GetHashCode() => Identity.CombineHash(Identity.CombineHashes(Aliases), FilePath);

    }

}
