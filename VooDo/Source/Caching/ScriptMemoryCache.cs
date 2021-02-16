
using SimpleLRUCache;

using VooDo.AST;
using VooDo.Parsing;

namespace VooDo.Caching
{
    public sealed class ScriptMemoryCache : IScriptCache
    {

        private readonly LRUCache<string, Script> m_cache;

        public ScriptMemoryCache(int _capacity = 128)
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

        public void Clear(string _source)
            => m_cache.Remove(_source.Trim());

        public void Clear()
            => m_cache.Clear();

        public Script GetOrParseScript(string _source)
        {
            _source = _source.Trim();
            if (m_cache.TryGetValue(_source, out Script? cached))
            {
                return cached;
            }
            Script script = Parser.Script(_source);
            m_cache.Add(_source, script);
            return script;
        }

    }
}
