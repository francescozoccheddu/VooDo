
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

using VooDo.Compiling;
using VooDo.WinUI.Core;
using VooDo.WinUI.Options;
using VooDo.WinUI.Xaml;

namespace VooDo.WinUI.Components
{

    public sealed class DefaultTargetProvider : ITargetProvider
    {

        private static Type? FirstPublicAncestor(Type _type)
        {
            Type? type = _type;
            while (type is not null && !type.IsPublic)
            {
                type = type.BaseType;
            }
            return type;
        }

        public Target? GetTarget(XamlInfo _xamlInfo)
        {
            List<Constant> constants = new List<Constant>();
            HashSet<Assembly> assemblies = new HashSet<Assembly>();
            if (_xamlInfo.Object is not null)
            {
                Type? type = FirstPublicAncestor(_xamlInfo.Object.GetType());
                if (type is not null)
                {
                    constants.Add(new Constant(type, "this", _xamlInfo.Object));
                    assemblies.Add(type.Assembly);
                }
            }
            object? root = _xamlInfo.Root ?? (_xamlInfo.Object as UIElement)?.XamlRoot?.Content;
            if (root is not null)
            {
                Type? type = FirstPublicAncestor(root.GetType());
                if (type is not null)
                {
                    constants.Add(new Constant(type, "root", root));
                    assemblies.Add(type.Assembly);
                }
            }
            Target.ReturnValueInfo? returnValueInfo = null;
            if (_xamlInfo.SourceKind == XamlInfo.ESourceKind.MarkupExtension)
            {
                ProvideValueTargetProperty property = _xamlInfo.Property!;
                Target.IReturnValueSetter setter;
                DependencyProperty? dependencyProperty = (DependencyProperty?) (_xamlInfo.Object as DependencyObject)
                    ?.GetType()
                    .GetProperty(
                        $"{property.Name}Property",
                        BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)?
                    .GetValue(null);
                if (dependencyProperty is not null)
                {
                    setter = new DependencyPropertySetter(dependencyProperty, (DependencyObject) _xamlInfo.Object!);
                }
                else
                {
                    PropertyInfo propertyInfo = _xamlInfo.Object!
                        .GetType()
                        .GetProperty(
                            _xamlInfo.Property!.Name,
                            BindingFlags.Instance | BindingFlags.Public)!;
                    setter = new ReflectionPropertySetter(propertyInfo, _xamlInfo.Object!);
                }
                assemblies.Add(property.Type.Assembly);
                returnValueInfo = new Target.ReturnValueInfo(property.Type, setter);
            }
            return new Target(returnValueInfo, BindingOptions.Empty with
            {
                Constants = constants.ToImmutableArray(),
                References = assemblies.Select(_a => Reference.FromAssembly(_a)).ToImmutableArray()
            });
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

        private sealed class ReflectionPropertySetter : Target.IReturnValueSetter
        {

            private readonly PropertyInfo m_property;
            private readonly object m_instance;

            public ReflectionPropertySetter(PropertyInfo _property, object _instance)
            {
                m_property = _property;
                m_instance = _instance;
            }

            public void SetReturnValue(object? _value) => m_property.SetValue(m_instance, _value);
        }

    }

}
