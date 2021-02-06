using Microsoft.UI.Xaml;

namespace VooDo.DesktopTestbench
{
    public sealed partial class App : Application
    {

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs _args)
        {
            m_window = new MainWindow
            {
                Title = "VooDo Desktop Testbench"
            };
            m_window.Activate();
        }

        private Window m_window;

    }
}
