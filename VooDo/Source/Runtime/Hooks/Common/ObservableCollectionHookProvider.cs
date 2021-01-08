using System.Collections.Specialized;

using VooDo.AST;

namespace VooDo.Runtime.Hooks.Common
{

    public sealed class NotifyCollectionChangedHookProvider : TypedHookProvider<INotifyCollectionChanged>
    {

        private sealed class Hook : IHook
        {

            internal Hook(INotifyCollectionChanged _binding, Name _name)
            {
                m_name = _name;
                m_instance = _binding;
                m_instance.CollectionChanged += NotifyEvalChange;
            }

            private void NotifyEvalChange(object _sender, NotifyCollectionChangedEventArgs _args) => OnChange?.Invoke();

            private readonly INotifyCollectionChanged m_instance;
            private readonly Name m_name;

            public event HookEventHandler OnChange;

            public void Unsubscribe() => m_instance.CollectionChanged -= NotifyEvalChange;

        }

        protected override IHook Subscribe(INotifyCollectionChanged _instance, Name _property)
        {
            if (_property == "Item")
            {
                return new Hook(_instance, _property);
            }
            else
            {
                return null;
            }
        }

    }

}
