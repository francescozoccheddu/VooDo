using Microsoft.UI.Xaml;

namespace VooDo.Testbench.Desktop
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs _args) => new MainWindow().Activate();
    }
}
