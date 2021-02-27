using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;

using System;
using System.IO;
using System.Linq;
using System.Reflection;

using VooDo.Runtime;
using VooDo.WinUI.Bindings;
using VooDo.WinUI.Utils;

namespace VooDo.WinUI.Xaml
{

    [ContentProperty(Name = nameof(Code))]
    public sealed class VooDo : MarkupExtension
    {

        private static readonly LRUCache<string, string> s_scriptCache = new(256);

        public VooDo() { }

        public VooDo(string _code)
        {
            Code = _code;
        }

        public string? Code { get; set; }
        public string? Path { get; set; }
        public string? Tag { get; set; }

        private static string GetCode(string _file)
        {
            if (!s_scriptCache.TryGetValue(_file, out string code))
            {
                s_scriptCache[_file] = code = File.ReadAllText(_file);
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
            IProvideValueTarget? provideValueTarget = (IProvideValueTarget?)_serviceProvider.GetService(typeof(IProvideValueTarget));
            IRootObjectProvider? rootObjectProvider = (IRootObjectProvider?)_serviceProvider.GetService(typeof(IRootObjectProvider));
            IUriContext? uriContext = (IUriContext?)_serviceProvider.GetService(typeof(IUriContext));
            if (provideValueTarget is null || rootObjectProvider is null || uriContext is null)
            {
                throw new InvalidOperationException("Failed to retrieve XAML target service");
            }
            if (provideValueTarget.TargetProperty is not ProvideValueTargetProperty property)
            {
                throw new InvalidOperationException("Failed to retrieve XAML target property");
            }
            object owner = provideValueTarget.TargetObject;
            object root = rootObjectProvider.RootObject;
            string xamlPath = uriContext.BaseUri.ToString();
            string code = Code ?? GetCode(Path!);
            PropertyBinder.Key key = new PropertyBinder.Key(code, property.Name, property.DeclaringType.FullName!);
            Loader loader = PropertyBinder.GetLoader(key, root.GetType());
            MemberInfo member = property.DeclaringType.GetMember(
                property.Name,
                MemberTypes.Field | MemberTypes.Property,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).Single();
            ITypedProgram program = (ITypedProgram)loader.Create();
            PropertyBinding? binding = member switch
            {
                PropertyInfo m => new PropertyBinding(program, m, owner, root, xamlPath, Tag ?? ""),
                FieldInfo m => new PropertyBinding(program, m, owner, root, xamlPath, Tag ?? ""),
                _ => throw new InvalidOperationException("Failed to retrieve original property value")
            };
            if (owner is FrameworkElement element)
            {
                element.Loaded += (_, _) => BindingManager.Add(binding);
                element.Loaded -= (_, _) => BindingManager.Remove(binding);
                if (element.IsLoaded)
                {
                    BindingManager.Add(binding);
                }
            }
            else
            {
                BindingManager.Add(binding);
            }
            return member switch
            {
                PropertyInfo m => m.GetValue(owner),
                FieldInfo m => m.GetValue(owner),
            };
        }

    }

}