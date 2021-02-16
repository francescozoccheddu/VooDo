using System;
using System.Collections.Generic;

using VooDo.AST;
using VooDo.Caching;
using VooDo.Compiling;
using VooDo.Hooks;
using VooDo.Runtime;
using VooDo.Utils;

namespace VooDo.WinUI
{

    public abstract class LoaderProvider : ILoaderProvider
    {

        protected abstract IHookInitializerProvider m_HookInitializerProvider { get; }
        protected abstract IEnumerable<Reference> m_References { get; }
        protected abstract IProgramCache m_ProgramCache { get; }

        public Loader GetLoader(Script _script, Type _type)
        {
            Loader? loader = m_ProgramCache.GetLoader(_key);
            if (loader is not null)
            {
                return loader;
            }
            Compilation compilation = Compilation.Create(_key.Script, _key.Options);
            if (!compilation.Succeded)
            {
                throw compilation.Problems.AsThrowable();
            }
            return m_ProgramCache.Cache(compilation);
        }

    }

}
