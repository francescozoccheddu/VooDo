using System;
using System.Collections.Generic;

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

        protected abstract IHookInitializerProvider GetHookInitializerProvider(Script _script, Target _target);
        protected abstract IEnumerable<Reference> GetReferences(Script _script, Target _target);
        protected virtual ILoaderCache? m_LoaderCache => null;
        protected virtual Identifier? GetAssemblyAlias(QualifiedType _qualifiedType)
        {
            return null;
        }

        public Loader GetLoader(Script _script, Target _target)
        {
            ComplexType? returnType = _target.ReturnType == typeof(void)
                ? null
                : GetTypeNode(_target.ReturnType);
            IEnumerable<Reference> references = GetReferences(_script, _target);
            IHookInitializerProvider hookInitializerProvider = GetHookInitializerProvider(_script, _target);
            LoaderKey key = LoaderKey.Create(_script, references, returnType, hookInitializerProvider);
            return m_LoaderCache?.GetOrCreateLoader(key)
                ?? Compilation.SucceedOrThrow(_script, key.CreateMatchingOptions()).Load();
        }

        public ComplexType GetTypeNode(Type _type)
        {
            ComplexType type = ComplexType.Parse(_type);
            return type.ReplaceNonNullDescendantNodes(_n =>
                _n is QualifiedType qualifiedType
                ? qualifiedType with { Alias = GetAssemblyAlias(qualifiedType) }
                : _n)!;
        }

    }

}
