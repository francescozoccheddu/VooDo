using System;
using System.Linq;

using VooDo.AST;
using VooDo.AST.Names;
using VooDo.Caching;
using VooDo.Compiling;
using VooDo.Runtime;
using VooDo.WinUI.Core;
using VooDo.WinUI.Interfaces;

namespace VooDo.WinUI.Components
{

    public sealed class SimpleLoaderProvider : ILoaderProvider<SimpleTarget>
    {

        public ILoaderCache LoaderCache { get; }

        public SimpleLoaderProvider(ILoaderCache _loaderCache)
        {
            LoaderCache = _loaderCache;
        }

        private static Identifier? GetAssemblyAlias(QualifiedType _qualifiedType)
        {
            Type? type = Type.GetType(_qualifiedType.ToString());
            if (type is not null)
            {
                string path = new Uri(type.Assembly.Location).AbsolutePath;
                return LoaderOptions.References
                    .FirstOrDefault(_r => _r.FilePath is not null && path == new Uri(_r.FilePath).AbsolutePath)?
                    .Aliases
                    .FirstOrDefault();
            }
            return null;
        }

        private static Script ProcessScript(Script _script)
        {
            return _script;
        }

        public Loader GetLoader(Script _script, SimpleTarget _target)
        {
            _script = ProcessScript(_script);
            ComplexType? returnType = _target.ReturnType == typeof(void)
                ? null
                : GetTypeNode(_target.ReturnType, _target);
            LoaderKey key = LoaderKey.Create(_script, LoaderOptions.References, returnType, LoaderOptions.HookInitializerProvider);
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
