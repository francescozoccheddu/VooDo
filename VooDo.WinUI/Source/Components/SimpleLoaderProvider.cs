using System;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST;
using VooDo.AST.Names;
using VooDo.Caching;
using VooDo.Compiling;
using VooDo.Hooks;
using VooDo.Runtime;
using VooDo.WinUI.Interfaces;

namespace VooDo.WinUI.Components
{

    public sealed class SimpleLoaderProvider : ILoaderProvider<SimpleTarget>
    {

        public ImmutableArray<Reference> References { get; }
        public IHookInitializerProvider HookInitializerProvider { get; }
        public ILoaderCache LoaderCache { get; }

        public SimpleLoaderProvider(ImmutableArray<Reference> _references, IHookInitializerProvider _hookInitializerProvider, ILoaderCache _loaderCache)
        {
            References = _references;
            HookInitializerProvider = _hookInitializerProvider;
            LoaderCache = _loaderCache;
        }

        private Identifier? GetAssemblyAlias(QualifiedType _qualifiedType)
        {
            Type? type = Type.GetType(_qualifiedType.ToString());
            if (type is not null)
            {
                string path = new Uri(type.Assembly.Location).AbsolutePath;
                return References
                    .FirstOrDefault(_r => _r.FilePath is not null && path == new Uri(_r.FilePath).AbsolutePath)?
                    .Aliases
                    .FirstOrDefault();
            }
            return null;
        }

        public Loader GetLoader(Script _script, SimpleTarget _target)
        {
            ComplexType? returnType = _target.ReturnType == typeof(void)
                ? null
                : GetTypeNode(_target.ReturnType, _target);
            LoaderKey key = LoaderKey.Create(_script, References, returnType, HookInitializerProvider);
            return LoaderCache?.GetOrCreateLoader(key)
                ?? Compilation.SucceedOrThrow(_script, key.CreateMatchingOptions()).Load();
        }

        public ComplexType GetTypeNode(Type _type, SimpleTarget _target)
        {
            ComplexType type = ComplexType.FromType(_type);
            return type.ReplaceNonNullDescendantNodes(_n =>
                _n is QualifiedType qualifiedType
                ? qualifiedType with { Alias = GetAssemblyAlias(qualifiedType) }
                : _n)!;
        }

    }

}
