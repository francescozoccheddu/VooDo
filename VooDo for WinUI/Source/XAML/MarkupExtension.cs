using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;

namespace VooDo.WinUI.Xaml
{

    public sealed class VooDo : MarkupExtension
    {

        public string? Text { get; set; }

        protected override object ProvideValue(IXamlServiceProvider _serviceProvider)
        {
            return $"Text is {Text}";
        }

    }

}
