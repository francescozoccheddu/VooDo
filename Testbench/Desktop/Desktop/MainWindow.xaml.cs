using Microsoft.UI.Xaml;

using System.Linq;

using VooDo.WinUI.Bindings;

namespace VooDo.Testbench.Desktop
{
    public sealed partial class MainWindow : Window
    {


        public MainWindow()
        {
            InitializeComponent();
            Activated += MainWindow_Activated;
            Closed += MainWindow_Closed;
            m_gridView.ItemsSource = Enumerable.Repeat(new Item() { Title = "Title", Price = 6.99f }, 100).ToArray();
        }

        private void MainWindow_Closed(object _sender, WindowEventArgs _args) => Binder.Unbind(this);
        private void MainWindow_Activated(object _sender, WindowActivatedEventArgs _args) => Binder.Bind(this);

    }
}
