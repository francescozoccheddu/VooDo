using System;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST;
using VooDo.AST.Names;
using VooDo.Caching;
using VooDo.Compiling;
using VooDo.Hooks;
using VooDo.Runtime;

namespace VooDo.WinUI
{

    public abstract class LoaderProvider : ILoaderProvider
    {

        protected abstract IHookInitializerProvider GetHookInitializerProvider(Target _target);
        protected abstract ImmutableArray<Reference> GetReferences(Target _target);
        protected virtual ILoaderCache? m_LoaderCache => null;
        protected virtual Identifier? GetAssemblyAlias(QualifiedType _qualifiedType, ImmutableArray<Reference> _references)
        {
            Type? type = Type.GetType(_qualifiedType.ToString());
            if (type is not null)
            {
                string path = new Uri(type.Assembly.Location).AbsolutePath;
                return _references
                    .FirstOrDefault(_r => _r.FilePath is not null && path == new Uri(_r.FilePath).AbsolutePath)?
                    .Aliases
                    .FirstOrDefault();
            }
            return null;
        }

        public Loader GetLoader(Script _script, Target _target)
        {
            ImmutableArray<Reference> references = GetReferences(_target);
            ComplexType? returnType = _target.ReturnType == typeof(void)
                ? null
                : GetTypeNode(_target.ReturnType, _target);
            IHookInitializerProvider hookInitializerProvider = GetHookInitializerProvider(_target);
            LoaderKey key = LoaderKey.Create(_script, references, returnType, hookInitializerProvider);
            return m_LoaderCache?.GetOrCreateLoader(key)
                ?? Compilation.SucceedOrThrow(_script, key.CreateMatchingOptions()).Load();
        }

        public ComplexType GetTypeNode(Type _type, Target _target)
        {
            ComplexType type = ComplexType.FromType(_type);
            ImmutableArray<Reference> references = GetReferences(_target);
            return type.ReplaceNonNullDescendantNodes(_n =>
                _n is QualifiedType qualifiedType
                ? qualifiedType with { Alias = GetAssemblyAlias(qualifiedType, references) }
                : _n)!;
        }

    }

}
