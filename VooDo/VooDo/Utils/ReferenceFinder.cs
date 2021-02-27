using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using VooDo.AST.Names;

namespace VooDo.Utils
{

    public static class ReferenceFinder
    {

        private static IAssemblySymbol? TryGetSymbol(MetadataReference _reference, Compilation _compilation)
            => _compilation.GetAssemblyOrModuleSymbol(_reference) as IAssemblySymbol;

        public static IEnumerable<MetadataReference> OrderByFileNameHint(IEnumerable<MetadataReference> _references, string _fileNameWithoutExt)
        {
            MetadataReference[] references = _references.ToArray();
            return references.Where(_r => HasFileName(_r, _fileNameWithoutExt)).Concat(references.Where(_r => !HasFileName(_r, _fileNameWithoutExt)));
        }

        public static IEnumerable<MetadataReference> FindByType(Type _type, Compilation _compilation)
            => FindByType(_type, _compilation, _compilation.References);

        public static IEnumerable<MetadataReference> FindByType(QualifiedType _type, Compilation _compilation)
            => FindByType(_type, _compilation, _compilation.References);

        public static IEnumerable<MetadataReference> FindByAssemblyName(string _assembly, Compilation _compilation)
            => FindByAssemblyName(_assembly, _compilation, _compilation.References);

        public static IEnumerable<MetadataReference> FindByNamespace(Namespace _namespace, Compilation _compilation)
            => FindByNamespace(_namespace, _compilation, _compilation.References);

        public static IEnumerable<MetadataReference> FindByFileName(string _fileNameWithoutExt, IEnumerable<MetadataReference> _references)
            => _references.Where(_r => HasFileName(_r, _fileNameWithoutExt));

        public static IEnumerable<MetadataReference> FindByType(Type _type, Compilation _compilation, IEnumerable<MetadataReference> _references)
            => FindByType(QualifiedType.FromType(_type, true), _compilation, OrderByFileNameHint(_references, Path.GetFileNameWithoutExtension(_type.Assembly.Location)));

        public static IEnumerable<MetadataReference> FindByType(QualifiedType _type, Compilation _compilation, IEnumerable<MetadataReference> _references)
            => _references.Where(_r => HasType(_r, _type, _compilation));

        public static IEnumerable<MetadataReference> FindByAssemblyName(Assembly _assembly, Compilation _compilation, IEnumerable<MetadataReference> _references)
            => FindByType(_assembly.GetName().Name, _compilation, OrderByFileNameHint(_references, Path.GetFileNameWithoutExtension(_assembly.Location)));

        public static IEnumerable<MetadataReference> FindByAssemblyName(string _assembly, Compilation _compilation, IEnumerable<MetadataReference> _references)
            => _references.Where(_r => GetAssemblyName(_r, _compilation) == _assembly);

        public static IEnumerable<MetadataReference> FindByNamespace(Namespace _namespace, Compilation _compilation, IEnumerable<MetadataReference> _references)
            => _references.Where(_r => HasNamespace(_r, _namespace, _compilation));

        public static bool HasFileName(MetadataReference _reference, string _fileNameWithoutExt)
            => _reference is PortableExecutableReference pe
                && pe.FilePath is not null
                && Path.GetFileNameWithoutExtension(pe.FilePath).Equals(_fileNameWithoutExt, FilePaths.SystemComparison);

        private static string GetMetadataName(SimpleType _type)
            => _type.IsGeneric
                ? $"{_type.Name}`{_type.TypeArguments.Length}"
                : $"{_type.Name}";

        private static string GetMetadataName(QualifiedType _type)
        {
            if (_type.IsArray || _type.IsNullable || _type.IsAliasQualified)
            {
                throw new ArgumentException("Type cannot be nullable, array or alias qualified", nameof(_type));
            }
            return string.Join(".", _type.Path.Select(GetMetadataName));
        }

        public static bool HasType(MetadataReference _reference, QualifiedType _type, Compilation _compilation)
            => TryGetSymbol(_reference, _compilation)?.GetTypeByMetadataName(GetMetadataName(_type)) is not null;

        public static bool HasNamespace(MetadataReference _reference, Namespace _namespace, Compilation _compilation)
        {
            string name = _namespace.ToString();
            HashSet<string> types = new HashSet<string>();
            IAssemblySymbol? symbol = TryGetSymbol(_reference, _compilation);
            if (symbol is null)
            {
                return false;
            }
            bool found = false;
            NamespaceVisitor.Visit(symbol, _n => !(found = _n.ToDisplayString() == name));
            return found;
        }

        public static string? GetAssemblyName(MetadataReference _reference, Compilation _compilation)
            => TryGetSymbol(_reference, _compilation)?.Name;

        public static ImmutableHashSet<Namespace>? GetNamespaces(MetadataReference _reference, Compilation _compilation)
        {
            IAssemblySymbol? symbol = TryGetSymbol(_reference, _compilation);
            if (symbol is null)
            {
                return null;
            }
            HashSet<string> namespaces = new HashSet<string>();
            NamespaceVisitor.Visit(symbol, _n => namespaces.Add(_n.ToDisplayString()) || true);
            return namespaces.Select(Namespace.Parse).ToImmutableHashSet();
        }

        private static string GetMetadataName(INamedTypeSymbol _symbol)
        {
            StringBuilder builder = new();
            INamespaceOrTypeSymbol? current = _symbol;
            while (current is not null)
            {
                if (!ReferenceEquals(current, _symbol))
                {
                    builder.Insert(0, '.');
                }
                builder.Insert(0, current.MetadataName);
                current = current.ContainingSymbol as INamespaceOrTypeSymbol;
            }
            return builder.ToString();
        }

        public static ImmutableArray<QualifiedType>? FindTypeByPartialName(QualifiedType _name, Compilation _compilation, MetadataReference _reference)
        {
            IAssemblySymbol? symbol = TryGetSymbol(_reference, _compilation);
            if (symbol is null)
            {
                return null;
            }
            string name = '.' + GetMetadataName(_name);
            Identifier alias = GetAlias(_reference);
            HashSet<string> types = new HashSet<string>();
            TypeVisitor.Visit(symbol, _t =>
            {
                if (GetMetadataName(_t).EndsWith(name))
                {
                    types.Add(_t.ToDisplayString());
                }
            });
            QualifiedType[] qTypes = types.Select(_t => QualifiedType.Parse(string.Concat(_t.TakeWhile(_c => _c != '`')))).ToArray();
            for (int q = 0; q < qTypes.Length; q++)
            {
                SimpleType[] path = qTypes[q].Path.ToArray();
                int count = _name.Path.Length;
                for (int s = 0; s < count; s++)
                {
                    ref SimpleType pathItem = ref path[path.Length - count + s];
                    pathItem = pathItem with
                    {
                        TypeArguments = _name.Path[s].TypeArguments
                    };
                }
                qTypes[q] = qTypes[q] with
                {
                    Alias = alias,
                    Path = path.ToImmutableArray()
                };
            }
            return qTypes.ToImmutableArray();
        }

        public static Identifier GetAlias(MetadataReference _reference)
            => _reference.Properties.Aliases.FirstOrDefault() ?? "global";

        private sealed class TypeVisitor : SymbolVisitor
        {

            internal static void Visit(IAssemblySymbol _symbol, Action<INamedTypeSymbol> _typeListener)
                => new TypeVisitor(_typeListener).Visit(_symbol.GlobalNamespace);

            private readonly Action<INamedTypeSymbol> m_typeListener;

            private TypeVisitor(Action<INamedTypeSymbol> _typeListener)
            {
                m_typeListener = _typeListener;
            }

            public override void VisitNamedType(INamedTypeSymbol _symbol)
            {
                m_typeListener(_symbol);
                foreach (INamedTypeSymbol c in _symbol.GetMembers().OfType<INamedTypeSymbol>())
                {
                    c.Accept(this);
                }
            }

            public override void VisitNamespace(INamespaceSymbol _symbol)
            {
                foreach (INamespaceOrTypeSymbol c in _symbol.GetMembers())
                {
                    c.Accept(this);
                }
            }

        }

        private sealed class NamespaceVisitor : SymbolVisitor
        {

            internal static void Visit(IAssemblySymbol _symbol, Predicate<INamespaceSymbol> _namespaceListener)
                => new NamespaceVisitor(_namespaceListener).Visit(_symbol.GlobalNamespace);

            private readonly Predicate<INamespaceSymbol> m_namespaceListener;

            private NamespaceVisitor(Predicate<INamespaceSymbol> _namespaceListener)
            {
                m_namespaceListener = _namespaceListener;
            }

            public override void VisitNamespace(INamespaceSymbol _symbol)
            {
                if (!m_namespaceListener?.Invoke(_symbol) ?? true)
                {
                    return;
                }
                foreach (INamespaceSymbol c in _symbol.GetNamespaceMembers())
                {
                    c.Accept(this);
                }
            }

        }

    }

}
