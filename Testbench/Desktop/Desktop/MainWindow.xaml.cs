﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

using System.Linq;

using VooDo.WinUI.Bindings;

namespace VooDo.Testbench.Desktop
{
    public sealed partial class MainWindow : Window
    {

        private readonly Status m_status = new();

        public MainWindow()
        {
            InitializeComponent();
            Activated += MainWindow_Activated;
            Closed += MainWindow_Closed;
            m_gridView.ItemsSource = Item.CreateItems(100);
            m_paneGridView.ItemsSource = Item.CreateItems(30);
        }

        private void MainWindow_Closed(object _sender, WindowEventArgs _args) => Binder.Unbind(this);
        private void MainWindow_Activated(object _sender, WindowActivatedEventArgs _args) => Binder.Bind(this);

    }
}
