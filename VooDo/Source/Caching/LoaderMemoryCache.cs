
using VooDo.Compiling;
using VooDo.Runtime;
using VooDo.Utils;

namespace VooDo.Caching
{

    public sealed class LoaderMemoryCache : ILoaderCache
    {

        private readonly LRUCache<LoaderKey, Loader> m_cache;

        public LoaderMemoryCache(int _capacity = 512)
        {
            m_cache = new(_capacity);
        }

        public int Capacity
        {
            get => m_cache.Capacity;
            set => m_cache.Capacity = value;
        }

        public void Trim(int _maxCount)
            => m_cache.Trim(_maxCount);

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
            Loader script = Compilation.SucceedOrThrow(_key.Script, _key.CreateMatchingOptions()).Load();
            m_cache.Add(_key, script);
            return script;
        }

    }

}
