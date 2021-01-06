using VooDo.AST;

namespace VooDo.Runtime.Hooks
{

    internal sealed class HookHolder
    {

        private readonly HookManager m_manager;
        private Name m_property;
        private object m_source;
        private IHook m_hook;

        public HookHolder(HookManager _manager) => m_manager = _manager;

        private void NotifyChange() => m_manager.NotifyChange();

        internal void Resubscribe(object _sourceValue, Name _property)
        {
            if (m_hook == null || !ReferenceEquals(m_source, _sourceValue) || m_property != _property)
            {
                m_property = _property;
                m_source = _sourceValue;
                Unsubscribe();
                foreach (IHookProvider provider in m_manager.HookProviders)
                {
                    m_hook = provider.Subscribe(m_source, m_property);
                    if (m_hook != null)
                    {
                        m_hook.OnChange += NotifyChange;
                        break;
                    }
                }
            }
        }

        internal void Unsubscribe()
        {
            if (m_hook != null)
            {
                m_hook.OnChange -= NotifyChange;
                m_hook.Unsubscribe();
                m_hook = null;
                m_source = null;
                m_property = null;
            }
        }

    }

}
