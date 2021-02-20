
using Microsoft.UI.Xaml;

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

    public sealed class DependencyPropertyTargetProvider : ITargetProvider
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
            if (_xamlInfo.Root is not null)
            {
                Type? type = FirstPublicAncestor(_xamlInfo.Root.GetType());
                if (type is not null)
                {
                    constants.Add(new Constant(type, "root", _xamlInfo.Root));
                    assemblies.Add(type.Assembly);
                }
            }
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
                    assemblies.Add(_xamlInfo.Property.Type.Assembly);
                    DependencyPropertySetter setter = new DependencyPropertySetter(dependencyProperty, owner);
                    Target.ReturnValueInfo returnValueInfo = new Target.ReturnValueInfo(_xamlInfo.Property.Type, setter);
                    return new Target(returnValueInfo, BindingOptions.Empty with
                    {
                        Constants = constants.ToImmutableArray(),
                        References = assemblies.Select(_a => Reference.FromAssembly(_a)).ToImmutableArray()
                    });
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
