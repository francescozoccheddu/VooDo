using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
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

        private static IEnumerable<MetadataReference> OrderByFileNameHint(IEnumerable<MetadataReference> _references, string _fileName)
        {
            MetadataReference[] references = _references.ToArray();
            return references.Where(_r => HasFileName(_r, _fileName)).Concat(references.Where(_r => !HasFileName(_r, _fileName)));
        }

        public static IEnumerable<MetadataReference> FindByType(string _type, Compilation _compilation)
            => FindByType(_type, _compilation, _compilation.References);

        public static IEnumerable<MetadataReference> FindByAssemblyName(string _assembly, Compilation _compilation)
            => FindByAssemblyName(_assembly, _compilation, _compilation.References);

        public static IEnumerable<MetadataReference> FindByNamespace(string _namespace, Compilation _compilation)
            => FindByNamespace(_namespace, _compilation, _compilation.References);

        public static IEnumerable<MetadataReference> FindByFileName(string _fileName, IEnumerable<MetadataReference> _references)
            => _references.Where(_r => HasFileName(_r, _fileName));

        public static IEnumerable<MetadataReference> FindByType(Type _type, Compilation _compilation, IEnumerable<MetadataReference> _references)
            => FindByType(_type.FullName, _compilation, OrderByFileNameHint(_references, Path.GetFileName(_type.Assembly.Location)));

        public static IEnumerable<MetadataReference> FindByType(string _type, Compilation _compilation, IEnumerable<MetadataReference> _references)
            => _references.Where(_r => HasType(_r, _type, _compilation));

        public static IEnumerable<MetadataReference> FindByAssemblyName(Assembly _assembly, Compilation _compilation, IEnumerable<MetadataReference> _references)
            => FindByType(_assembly.GetName().Name, _compilation, OrderByFileNameHint(_references, Path.GetFileName(_assembly.Location)));

        public static IEnumerable<MetadataReference> FindByAssemblyName(string _assembly, Compilation _compilation, IEnumerable<MetadataReference> _references)
            => _references.Where(_r => GetAssemblyName(_r, _compilation) == _assembly);

        public static IEnumerable<MetadataReference> FindByNamespace(string _namespace, Compilation _compilation, IEnumerable<MetadataReference> _references)
            => _references.Where(_r => GetNamespaces(_r, _compilation).Contains(_namespace));

        public static bool HasFileName(MetadataReference _reference, string _fileName)
            => _reference is PortableExecutableReference pe
                && pe.FilePath is not null
                && Path.GetFileNameWithoutExtension(pe.FilePath).Equals(_fileName, System.StringComparison.OrdinalIgnoreCase);

        public static bool HasType(MetadataReference _reference, string _type, Compilation _compilation)
            => TryGetSymbol(_reference, _compilation)?.GetTypeByMetadataName(_type) is not null;

        public static string? GetAssemblyName(MetadataReference _reference, Compilation _compilation)
            => TryGetSymbol(_reference, _compilation)?.Name;

        public static IEnumerable<string>? GetTypes(MetadataReference _reference, Compilation _compilation)
            => TryGetSymbol(_reference, _compilation)?.TypeNames;

        public static IEnumerable<string>? GetNamespaces(MetadataReference _reference, Compilation _compilation)
            => TryGetSymbol(_reference, _compilation)?.NamespaceNames;

        public static Identifier GetAlias(MetadataReference _reference)
            => _reference.Properties.Aliases.FirstOrDefault() ?? "global";

    }

}
