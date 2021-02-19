using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;

using System;

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

        protected override object ProvideValue(IXamlServiceProvider _serviceProvider)
        {
            if (Binding is null)
            {
                IProvideValueTarget provideValueTarget = (IProvideValueTarget) _serviceProvider.GetService(typeof(IProvideValueTarget));
                IRootObjectProvider rootObjectProvider = (IRootObjectProvider) _serviceProvider.GetService(typeof(IRootObjectProvider));
                IUriContext uriContext = (IUriContext) _serviceProvider.GetService(typeof(IUriContext));
                if (Code is null)
                {
                    throw new InvalidOperationException("Code is null");
                }
                XamlInfo xamlInfo = new XamlInfo(rootObjectProvider.RootObject, provideValueTarget.TargetObject, provideValueTarget.TargetProperty, uriContext.BaseUri, Code);
                Binding = CoreBindingManager.BindingManager.AddBinding(xamlInfo);
                if (Binding.Program is TypedProgram typedProgram)
                {
                    typedProgram.OnReturn += ProgramReturned;
                }
                Binding.Program.RequestRun();
            }
            return m_lastValue!;
        }

        private void ProgramReturned(object? _value) => m_lastValue = _value;
    }

}
