using Microsoft.UI.Xaml;

using System.Reflection;

using VooDo.Runtime;

namespace VooDo.WinUI.Hooks
{

    public sealed class DependencyPropertyHook : Hook<DependencyObject, (DependencyObject obj, long token), object?>
    {

        private readonly string m_name;
        private DependencyProperty? m_property;

        public DependencyPropertyHook(string _name)
        {
            m_name = _name;
        }

        public override IHook Clone() => new DependencyPropertyHook(m_name);

        protected override (DependencyObject obj, long token) Subscribe(DependencyObject _object)
        {
            if (m_property is null)
            {
                m_property = (DependencyProperty)_object
                    .GetType()
                    .GetProperty($"{m_name}Property", BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Static)!
                    .GetValue(null)!;
            }
            return (_object, _object.RegisterPropertyChangedCallback(m_property, PropertyChanged));
        }

        protected override void Unsubscribe((DependencyObject obj, long token) _token)
            => _token.obj.UnregisterPropertyChangedCallback(m_property, _token.token);

        private void PropertyChanged(DependencyObject? _sender, DependencyProperty _property)
            => NotifyChange(_sender!.GetValue(_property));

    }

}
