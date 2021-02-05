using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;

using VooDo.Utils;

namespace VooDo.Factory
{

    public sealed class Reference : IEquatable<Reference>
    {

        public static Reference FromStream(Stream _stream, params Identifier[] _aliases)
            => FromStream(_stream, _aliases);

        public static Reference FromFile(string _path, params Identifier[] _aliases)
            => FromFile(_path, _aliases);

        public static Reference FromImage(IEnumerable<byte> _image, params Identifier[] _aliases)
            => FromImage(_image, _aliases);

        public static Reference FromAssembly(Assembly _assembly, params Identifier[] _aliases)
            => FromAssembly(_assembly, (IEnumerable<Identifier>) _aliases);

        public static Reference FromStream(Stream _stream, IEnumerable<Identifier> _aliases = null)
        {
            if (_stream == null)
            {
                throw new ArgumentNullException(nameof(_stream));
            }
            return new Reference(MetadataReference.CreateFromStream(_stream), _aliases);
        }

        public static Reference FromFile(string _path, IEnumerable<Identifier> _aliases = null)
        {
            if (_path == null)
            {
                throw new ArgumentNullException(nameof(_path));
            }
            return new Reference(MetadataReference.CreateFromFile(_path), _aliases);
        }

        public static Reference FromImage(IEnumerable<byte> _bytes, IEnumerable<Identifier> _aliases = null)
        {
            if (_bytes == null)
            {
                throw new ArgumentNullException(nameof(_bytes));
            }
            return new Reference(MetadataReference.CreateFromImage(_bytes), _aliases);
        }

        public static Reference FromAssembly(Assembly _assembly, IEnumerable<Identifier> _aliases = null)
        {
            if (_assembly == null)
            {
                throw new ArgumentNullException(nameof(_assembly));
            }
            return FromFile(_assembly.Location, _aliases ?? Enumerable.Empty<Identifier>());
        }

        private Reference(PortableExecutableReference _metadata, IEnumerable<Identifier> _aliases = null)
        {
            if (_metadata == null)
            {
                throw new ArgumentNullException(nameof(_metadata));
            }
            Aliases = _aliases.EmptyIfNull().ToImmutableHashSet();
            if (Aliases.AnyNull())
            {
                throw new ArgumentException("Null alias", nameof(_aliases));
            }
            m_metadata = _metadata;
        }

        private readonly PortableExecutableReference m_metadata;

        public ImmutableHashSet<Identifier> Aliases { get; }
        public string FilePath => m_metadata?.FilePath;
        public string DisplayName => m_metadata?.Display;

        public Reference WithAliases(params Identifier[] _aliases)
            => WithAliases((IEnumerable<Identifier>) _aliases);

        public Reference WithAliases(IEnumerable<Identifier> _aliases = null)
            => new Reference(m_metadata, _aliases);

        internal MetadataReference GetMetadataReference() => m_metadata.WithAliases(Aliases.Cast<string>());

        public override bool Equals(object _obj) => Equals(_obj as Reference);
        public bool Equals(Reference _other) => _other != null && m_metadata.Equals(_other.m_metadata) && Aliases.SetEquals(_other.Aliases);
        public override int GetHashCode() => m_metadata.GetHashCode();
        public static bool operator ==(Reference _left, Reference _right) => Identity.AreEqual(_left, _right);
        public static bool operator !=(Reference _left, Reference _right) => !(_left == _right);
        public override string ToString()
            => $"{{{nameof(Reference)}: {DisplayName ?? FilePath ?? m_metadata.GetMetadataId().ToString()}}}";

    }

}
