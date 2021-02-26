using Microsoft.UI.Xaml;

using VooDo.WinUI.Bindings;

namespace VooDo.DesktopTestbench
{
    public sealed partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            Activated += MainWindow_Activated;
            Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object _sender, WindowEventArgs _args) => ClassBinder.Unbind(this);
        private void MainWindow_Activated(object _sender, WindowActivatedEventArgs _args) => ClassBinder.Bind(this);
    }
}
