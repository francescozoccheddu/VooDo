using Microsoft.UI.Xaml;

using System;
using System.Runtime.InteropServices;

namespace VooDo.Testbench.Desktop
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs _args)
        {
            MainWindow? window = new()
            {
                Title = "VooDo Testbench (Desktop)"
            };
            window.Activate();
        }

    }
}
