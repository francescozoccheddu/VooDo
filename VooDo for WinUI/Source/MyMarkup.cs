using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;

namespace VooDo.WinUI
{

    [MarkupExtensionReturnType(ReturnType = typeof(bool?))]
    public sealed class MyMarkup : MarkupExtension
    {

        public string? Text { get; set; }

        protected override object ProvideValue(IXamlServiceProvider _serviceProvider)
        {
            return $"Text is {Text}";
        }

    }

}
