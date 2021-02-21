using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using VooDo.Runtime;
using VooDo.Utils;
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
        private object? m_lastValue;

        private static readonly Dictionary<string, string> s_codeCache = new Dictionary<string, string>();

        private static string GetCode(string _path)
        {
            _path = NormalizeFilePath.Normalize(_path);
            if (!s_codeCache.TryGetValue(_path, out string? code))
            {
                s_codeCache[_path] = code = File.ReadAllText(_path);
            }
            return code;
        }

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
            if (Binding is null)
            {
                string code = Code ?? GetCode(Path!);
                IProvideValueTarget? provideValueTarget = (IProvideValueTarget?) _serviceProvider.GetService(typeof(IProvideValueTarget));
                IRootObjectProvider? rootObjectProvider = (IRootObjectProvider?) _serviceProvider.GetService(typeof(IRootObjectProvider));
                IUriContext? uriContext = (IUriContext?) _serviceProvider.GetService(typeof(IUriContext));
                if (provideValueTarget is null || rootObjectProvider is null || uriContext is null)
                {
                    throw new InvalidOperationException("Failed to retrieve XAML service");
                }
                if (provideValueTarget.TargetProperty is not ProvideValueTargetProperty property)
                {
                    throw new InvalidOperationException("Failed to retrieve XAML target property");
                }
                XamlInfo xamlInfo = new XamlInfo(code, property, provideValueTarget.TargetObject, rootObjectProvider.RootObject, uriContext.BaseUri);
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
