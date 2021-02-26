using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;

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

        public static IEnumerable<MetadataReference> FindByType(string _type, Compilation _compilation)
            => FindByType(_type, _compilation, _compilation.References);

        public static IEnumerable<MetadataReference> FindByAssemblyName(string _assembly, Compilation _compilation)
            => FindByAssemblyName(_assembly, _compilation, _compilation.References);

        public static IEnumerable<MetadataReference> FindByNamespace(string _namespace, Compilation _compilation)
            => FindByNamespace(_namespace, _compilation, _compilation.References);

        public static IEnumerable<MetadataReference> FindByFileName(string _fileNameWithoutExt, IEnumerable<MetadataReference> _references)
            => _references.Where(_r => HasFileName(_r, _fileNameWithoutExt));

        public static IEnumerable<MetadataReference> FindByType(Type _type, Compilation _compilation, IEnumerable<MetadataReference> _references)
            => FindByType(_type.FullName, _compilation, OrderByFileNameHint(_references, Path.GetFileNameWithoutExtension(_type.Assembly.Location)));

        public static IEnumerable<MetadataReference> FindByType(string _type, Compilation _compilation, IEnumerable<MetadataReference> _references)
            => _references.Where(_r => HasType(_r, _type, _compilation));

        public static IEnumerable<MetadataReference> FindByAssemblyName(Assembly _assembly, Compilation _compilation, IEnumerable<MetadataReference> _references)
            => FindByType(_assembly.GetName().Name, _compilation, OrderByFileNameHint(_references, Path.GetFileNameWithoutExtension(_assembly.Location)));

        public static IEnumerable<MetadataReference> FindByAssemblyName(string _assembly, Compilation _compilation, IEnumerable<MetadataReference> _references)
            => _references.Where(_r => GetAssemblyName(_r, _compilation) == _assembly);

        public static IEnumerable<MetadataReference> FindByNamespace(string _namespace, Compilation _compilation, IEnumerable<MetadataReference> _references)
            => _references.Where(_r => HasNamespace(_r, _namespace, _compilation));

        public static bool HasFileName(MetadataReference _reference, string _fileNameWithoutExt)
            => _reference is PortableExecutableReference pe
                && pe.FilePath is not null
                && Path.GetFileNameWithoutExtension(pe.FilePath).Equals(_fileNameWithoutExt, StringComparison.OrdinalIgnoreCase);

        public static bool HasType(MetadataReference _reference, string _type, Compilation _compilation)
            => TryGetSymbol(_reference, _compilation)?.GetTypeByMetadataName(_type) is not null;

        public static bool HasNamespace(MetadataReference _reference, string _type, Compilation _compilation)
        {
            HashSet<string> types = new HashSet<string>();
            IAssemblySymbol? symbol = TryGetSymbol(_reference, _compilation);
            if (symbol is null)
            {
                return false;
            }
            bool found = false;
            NamespaceVisitor.Visit(symbol, _n => !(found = _n.ToDisplayString() == _type));
            return found;
        }

        public static string? GetAssemblyName(MetadataReference _reference, Compilation _compilation)
            => TryGetSymbol(_reference, _compilation)?.Name;

        public static ImmutableHashSet<string>? GetNamespaces(MetadataReference _reference, Compilation _compilation)
        {
            HashSet<string> namespaces = new HashSet<string>();
            IAssemblySymbol? symbol = TryGetSymbol(_reference, _compilation);
            if (symbol is null)
            {
                return null;
            }
            NamespaceVisitor.Visit(symbol, _n => namespaces.Add(_n.ToDisplayString()) || true);
            return namespaces.ToImmutableHashSet();
        }


        public static Identifier GetAlias(MetadataReference _reference)
            => _reference.Properties.Aliases.FirstOrDefault() ?? "global";

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
                foreach (INamespaceOrTypeSymbol c in _symbol.GetNamespaceMembers())
                {
                    c.Accept(this);
                }
            }

        }

    }

}
