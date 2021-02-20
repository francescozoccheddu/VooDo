using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;

using System;
using System.Linq;
using System.Reflection;

using VooDo.Runtime;
using VooDo.WinUI.Core;

namespace VooDo.WinUI.Xaml
{

    [ContentProperty(Name = nameof(Code))]
    public sealed class VooDo : MarkupExtension
    {

        public VooDo() { }

        public VooDo(string _code)
        {
            Code = _code;
        }

        public string? Code { get; set; }

        public Binding? Binding { get; private set; }
        private object? m_lastValue;

        protected override object? ProvideValue(IXamlServiceProvider _serviceProvider)
        {
            if (Binding is null)
            {
                if (Code is null)
                {
                    throw new InvalidOperationException("Code is null");
                }
                IProvideValueTarget? provideValueTarget = (IProvideValueTarget?) _serviceProvider.GetService(typeof(IProvideValueTarget));
                IRootObjectProvider? rootObjectProvider = (IRootObjectProvider?) _serviceProvider.GetService(typeof(IRootObjectProvider));
                IUriContext? uriContext = (IUriContext?) _serviceProvider.GetService(typeof(IUriContext));
                if (provideValueTarget is null || rootObjectProvider is null || uriContext is null)
                {
                    throw new InvalidOperationException("Failed to retrieve XAML service");
                }
                ProvideValueTargetProperty? property = provideValueTarget.TargetProperty as ProvideValueTargetProperty;
                if (property is null)
                {
                    throw new InvalidOperationException("Failed to retrieve XAML target property");
                }
                XamlInfo xamlInfo = new XamlInfo(Code, property, provideValueTarget.TargetObject, rootObjectProvider.RootObject, uriContext.BaseUri);
                Binding = BindingManager.AddBinding(xamlInfo);
                if (Binding.Program is TypedProgram typedProgram)
                {
                    typedProgram.OnReturn += ProgramReturned;
                    Binding.Program.RequestRun();
                }
                else
                {
                    MemberInfo[] members = property.DeclaringType.GetMember(
                        property.Name,
                        MemberTypes.Field | MemberTypes.Property,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                    return members.Single() switch
                    {
                        PropertyInfo m => m.GetValue(provideValueTarget.TargetObject),
                        FieldInfo m => m.GetValue(provideValueTarget.TargetObject),
                        _ => throw new InvalidOperationException("Failed to retrieve original property value")
                    };
                }
            }
            return m_lastValue;
        }

        private void ProgramReturned(object? _value)
        {
            m_lastValue = _value;
        }
    }

}
