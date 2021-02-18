
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;

using System.Reflection;

using VooDo.WinUI.Interfaces;
using VooDo.WinUI.Xaml;

namespace VooDo.WinUI.Components
{

    public sealed class DependencyPropertyTargetProvider : ITargetProvider<SimpleTarget>
    {

        public SimpleTarget? GetTarget(XamlInfo _xamlInfo)
        {
            if (_xamlInfo.TargetObject is DependencyObject owner && _xamlInfo.TargetProperty is ProvideValueTargetProperty property)
            {
                DependencyProperty dependencyProperty = (DependencyProperty) owner
                    .GetType()
                    .GetProperty(
                        $"{property.Name}Property",
                        BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)!
                    .GetValue(null)!;
                return new SimpleTarget(new DependencyPropertyReturnTarget(dependencyProperty, owner));
            }
            return null;
        }


        private sealed class DependencyPropertyReturnTarget : ReturnTarget<object>
        {

            private readonly DependencyProperty m_property;
            private readonly DependencyObject m_object;

            public DependencyPropertyReturnTarget(DependencyProperty _property, DependencyObject _object)
            {
                m_property = _property;
                m_object = _object;
            }

            protected override object m_Value { set => m_object.SetValue(m_property, value); }

        }

    }

}
