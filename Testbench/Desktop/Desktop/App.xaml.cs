using Microsoft.UI.Xaml;

namespace VooDo.DesktopTestbench
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
