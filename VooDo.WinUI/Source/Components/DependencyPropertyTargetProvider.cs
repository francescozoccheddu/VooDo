
using Microsoft.UI.Xaml;

using System.Reflection;

using VooDo.WinUI.Core;
using VooDo.WinUI.Xaml;

namespace VooDo.WinUI.Components
{

    public sealed class DependencyPropertyTargetProvider : ITargetProvider
    {

        public Target? GetTarget(XamlInfo _xamlInfo)
        {
            if (_xamlInfo.Object is DependencyObject owner)
            {
                DependencyProperty? dependencyProperty = (DependencyProperty?) owner
                    .GetType()
                    .GetProperty(
                        $"{_xamlInfo.Property.Name}Property",
                        BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)?
                    .GetValue(null);
                if (dependencyProperty is not null)
                {
                    DependencyPropertySetter setter = new DependencyPropertySetter(dependencyProperty, owner);
                    Target.ReturnValueInfo returnValueInfo = new Target.ReturnValueInfo(_xamlInfo.Property.Type, setter);
                    return new Target(returnValueInfo);
                }
            }
            return null;
        }


        private sealed class DependencyPropertySetter : Target.IReturnValueSetter
        {

            private readonly DependencyProperty m_property;
            private readonly DependencyObject m_object;

            internal DependencyPropertySetter(DependencyProperty _property, DependencyObject _object)
            {
                m_property = _property;
                m_object = _object;
            }

            public void SetReturnValue(object? _value) => m_object.SetValue(m_property, _value);

        }

    }

}
