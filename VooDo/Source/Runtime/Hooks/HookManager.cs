using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using VooDo.AST;
using VooDo.AST.Expressions;
using VooDo.Runtime.Hooks;
using VooDo.Runtime.Hooks.Common;
using VooDo.Utils;

namespace VooDo.Runtime
{
    public sealed class HookManager
    {

        public interface IHookProviderSelector
        {
            IHook Subscribe(HookManager _hookManager, Eval _souce, Name _property);
        }

        public sealed class SimpleHookProvider : IHookProviderSelector
        {

            IHook IHookProviderSelector.Subscribe(HookManager _hookManager, Eval _source, Name _property)
            {
                IHook hook = null;
                if (!_source.IsNull)
                {
                    IHookProvider[] matches = _hookManager.HookProviders.Where(_p => _source.Type.IsAssignableFrom(_p.Type)).ToArray();
                    matches = matches.OrderByDescending(_p => matches.Count(_pi => _pi.Type.IsAssignableFrom(_p.Type))).ToArray();
                    foreach (IHookProvider match in matches)
                    {
                        hook = match.Subscribe(_source, _property);
                        if (hook != null)
                        {
                            break;
                        }
                    }
                }
                return hook;
            }

        }

        public HookManager(Script _script)
        {
            Ensure.NonNull(_script, nameof(_script));
            Script = _script;
            HookProviders = new List<IHookProvider>() { new EnvHookProvider(), new DependencyObjectHookProvider(), new NotifyPropertyChangedHookProvider(), new NotifyCollectionChangedHookProvider() };
            m_firedEvents = new Queue<EventInfo>();
            m_hooks = new Dictionary<Expr, HookHolder>(new Identity.ReferenceComparer<Expr>());
            HookProviderSelector = new SimpleHookProvider();
        }

        internal EventInfo CurrentEvent
            => m_firedEvents.TryPeek(out EventInfo current) ? current : null;

        private readonly Queue<EventInfo> m_firedEvents;
        private readonly Dictionary<Expr, HookHolder> m_hooks;

        public IHookProviderSelector HookProviderSelector { get; set; }

        public Script Script { get; }

        public List<IHookProvider> HookProviders { get; }

        internal void NotifyChange() => Script.RequestRun();

        private HookHolder GetHook(Expr _expr)
        {
            if (!m_hooks.TryGetValue(_expr, out HookHolder hook))
            {
                hook = new HookHolder(this);
                m_hooks.Add(_expr, hook);
            }
            return hook;
        }

        internal void Subscribe(Expr _expr, Eval _source, Name _property) => GetHook(_expr).Resubscribe(_source, _property);

        internal void Unsubscribe(Expr _expr) => GetHook(_expr).Unsubscribe();

    }
}
