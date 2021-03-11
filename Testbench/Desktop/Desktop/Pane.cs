using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace VooDo.Testbench.Desktop
{
    internal sealed class Pane : Grid
    {

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

#pragma warning disable IDE1006 // Naming Styles

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(Pane), new PropertyMetadata(false));

#pragma warning restore IDE1006 // Naming Styles


    }
}
