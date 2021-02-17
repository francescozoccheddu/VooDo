using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;

using System;

namespace VooDo.WinUI.Xaml
{

    public sealed class VooDo : MarkupExtension
    {

        public string? Text { get; set; }

        protected override object ProvideValue(IXamlServiceProvider _serviceProvider)
        {
            IProvideValueTarget provideValueTarget = (IProvideValueTarget) _serviceProvider.GetService(typeof(IProvideValueTarget));
            if (provideValueTarget.TargetProperty is ProvideValueTargetProperty targetProperty)
            {
                return 2;
            }
            else
            {
                throw new NotSupportedException("Property not supported");
            }
        }

    }

}
