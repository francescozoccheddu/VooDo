using System.Collections.Generic;

namespace VooDo.Generator
{

    internal sealed class NameDictionary
    {

        private readonly Dictionary<string, int> m_names = new();

        internal string TakeName(string _name)
        {
            int count = m_names.TryGetValue(_name, out int value) ? value : 0;
            m_names[_name] = count++;
            if (count > 1)
            {
                _name = $"{_name}{count}";
            }
            return _name;
        }

    }

}
