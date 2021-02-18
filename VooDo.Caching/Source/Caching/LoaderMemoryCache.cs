
using System.Collections.Generic;

using VooDo.Compiling;
using VooDo.Runtime;

namespace VooDo.Caching
{

    public sealed class LoaderMemoryCache : ILoaderCache
    {

        private readonly Dictionary<LoaderKey, Loader> m_cache;

        public LoaderMemoryCache(int _capacity = 512)
        {
            m_cache = new(_capacity);
        }

        public void Clear(LoaderKey _key)
            => m_cache.Remove(_key);

        public void Clear()
            => m_cache.Clear();

        public Loader GetOrCreateLoader(LoaderKey _key)
        {
            if (m_cache.TryGetValue(_key, out Loader? cached))
            {
                return cached;
            }
            Loader loader = Compilation.SucceedOrThrow(_key.Script, _key.CreateMatchingOptions()).Load();
            m_cache.Add(_key, loader);
            return loader;
        }

    }

}
