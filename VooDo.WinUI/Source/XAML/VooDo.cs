using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;

using System;

using VooDo.WinUI.Core;

namespace VooDo.WinUI.Xaml
{

    [ContentProperty(Name = nameof(Code))]
    public sealed class VooDo : MarkupExtension
    {

        public string? Code { get; set; }

        public Binding? Binding { get; private set; }

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
            }
            return null!;
        }

    }

}
