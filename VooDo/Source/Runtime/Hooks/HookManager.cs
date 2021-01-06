using System.Collections.Generic;
using System.Reflection;

using VooDo.AST;
using VooDo.AST.Expressions;
using VooDo.Runtime.Hooks;
using VooDo.Utils;

namespace VooDo.Runtime
{
    public sealed class HookManager
    {

        public HookManager(Script _script)
        {
            Ensure.NonNull(_script, nameof(_script));
            Script = _script;
            HookProviders = new List<IHookProvider>();
            m_firedEvents = new Queue<EventInfo>();
            m_hooks = new Dictionary<Expr, HookHolder>(new Identity.ReferenceComparer<Expr>());
        }

        internal EventInfo CurrentEvent
            => m_firedEvents.TryPeek(out EventInfo current) ? current : null;

        private readonly Queue<EventInfo> m_firedEvents;
        private readonly Dictionary<Expr, HookHolder> m_hooks;

        public Script Script { get; }

        public List<IHookProvider> HookProviders { get; }

        internal void NotifyChange()
        {

        }

        private HookHolder GetHook(Expr _expr)
        {
            if (!m_hooks.TryGetValue(_expr, out HookHolder hook))
            {
                hook = new HookHolder(this);
                m_hooks.Add(_expr, hook);
            }
            return hook;
        }

        internal void Subscribe(Expr _expr, object _source, Name _property) => GetHook(_expr).Resubscribe(_source, _property);

        internal void Unsubscribe(Expr _expr) => GetHook(_expr).Unsubscribe();

    }
}
