
using System.ComponentModel;

using VooDo.AST;

namespace VooDo.Runtime.Hooks.Common
{

    public sealed class NotifyPropertyChangedHookProvider : TypedHookProvider<INotifyPropertyChanged>
    {

        private sealed class Hook : IHook
        {

            internal Hook(INotifyPropertyChanged _binding, Name _name)
            {
                m_name = _name;
                m_instance = _binding;
                m_instance.PropertyChanged += NotifyEvalChange;
            }

            private void NotifyEvalChange(object _sender, PropertyChangedEventArgs _args)
            {
                if (_args.PropertyName == m_name)
                {
                    OnChange?.Invoke();
                }
            }

            private readonly INotifyPropertyChanged m_instance;
            private readonly Name m_name;

            public event HookEventHandler OnChange;

            public void Unsubscribe() => m_instance.PropertyChanged -= NotifyEvalChange;

        }

        protected override IHook Subscribe(INotifyPropertyChanged _instance, Name _property) => new Hook(_instance, _property);

    }

}
