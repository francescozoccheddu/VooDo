using System.Reflection;

using VooDo.AST;

using Windows.UI.Xaml;

namespace VooDo.Runtime.Hooks.Common
{

    public sealed class DependencyObjectHookProvider : TypedHookProvider<DependencyObject>
    {

        private sealed class Hook : IHook
        {

            internal Hook(DependencyObject _object, DependencyProperty _property)
            {
                m_object = _object;
                m_property = _property;
                m_token = _object.RegisterPropertyChangedCallback(_property, NotifyEvalChange);
            }

            private void NotifyEvalChange(DependencyObject _sender, DependencyProperty _property) => OnChange?.Invoke();

            private readonly DependencyObject m_object;
            private readonly DependencyProperty m_property;
            private readonly long m_token;

            public event HookEventHandler OnChange;

            public void Unsubscribe()
                => m_object.UnregisterPropertyChangedCallback(m_property, m_token);

        }

        protected override IHook Subscribe(DependencyObject _instance, Name _property)
        {
            if (_instance.GetType().GetField($"{_property}Property", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)?.GetValue(null) is DependencyProperty property)
            {
                return new Hook(_instance, property);
            }
            else
            {
                return null;
            }
        }

    }

}
