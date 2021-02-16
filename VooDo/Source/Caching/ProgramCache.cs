using System.Collections.Generic;
using System.Collections.Immutable;

using VooDo.Runtime;

namespace VooDo.Caching
{

    public sealed partial class ProgramCache : IProgramCache
    {

        private readonly Dictionary<ProgramKey, Loader> m_dictionary = new Dictionary<ProgramKey, Loader>();

        public void Cache(ProgramKey _key, Loader _loader)
        {
            m_dictionary[_key] = _loader;
        }

        public Loader? this[ProgramKey _key] => m_dictionary.GetValueOrDefault(_key);

        public int Count => m_dictionary.Count;

        public void Clear(ProgramKey _key)
        {
            m_dictionary.Remove(_key);
        }

        public void Clear()
        {
            m_dictionary.Clear();
        }

        Loader? IProgramCache.GetLoader(ProgramKey _key)
            => this[_key];

    }

}
