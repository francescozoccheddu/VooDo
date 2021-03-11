using Microsoft.UI.Xaml.Controls;

namespace VooDo.Testbench.Desktop
{
    internal sealed class CustomGridView : GridView
    {

        public ScrollViewer ScrollViewer { get; private set; } = null!;

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ScrollViewer = (ScrollViewer)GetTemplateChild("ScrollViewer");
        }
    }
}
