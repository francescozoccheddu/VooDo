using Microsoft.UI.Xaml;

namespace VooDo.Testbench.Desktop
{
    public sealed partial class App : Application
    {

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs _args)
        {
            new MainWindow
            {
                Title = "VooDo Desktop Testbench"
            }.Activate();
        }


    }
}
