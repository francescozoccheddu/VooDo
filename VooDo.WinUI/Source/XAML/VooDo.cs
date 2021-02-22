using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;

using System;
using System.Linq;
using System.Reflection;

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
        public string? Path { get; set; }

        public Binding? Binding { get; private set; }

        protected override object? ProvideValue(IXamlServiceProvider _serviceProvider)
        {
            if (Code is null && Path is null)
            {
                throw new InvalidOperationException("No code or path specified");
            }
            if (Code is not null && Path is not null)
            {
                throw new InvalidOperationException("Cannot specify both Code and Path");
            }
            IProvideValueTarget? provideValueTarget = (IProvideValueTarget?) _serviceProvider.GetService(typeof(IProvideValueTarget));
            IRootObjectProvider? rootObjectProvider = (IRootObjectProvider?) _serviceProvider.GetService(typeof(IRootObjectProvider));
            IUriContext? uriContext = (IUriContext?) _serviceProvider.GetService(typeof(IUriContext));
            if (provideValueTarget is null || rootObjectProvider is null || uriContext is null)
            {
                throw new InvalidOperationException("Failed to retrieve XAML target service");
            }
            if (provideValueTarget.TargetProperty is not ProvideValueTargetProperty property)
            {
                throw new InvalidOperationException("Failed to retrieve XAML target property");
            }
            if (Binding is null)
            {
                string code = Code ?? CodeLoader.GetCode(Path!);
                XamlInfo xamlInfo = XamlInfo.FromMarkupExtension(code, property, provideValueTarget.TargetObject, rootObjectProvider.RootObject, uriContext.BaseUri);
                Binding = BindingManager.CreateBinding(xamlInfo);
                BindingManager.AddBinding(Binding);
            }
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

}
