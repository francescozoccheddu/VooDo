using VooDo.AST;
using VooDo.AST.Statements;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using VooDo.Parsing;

namespace VooDoTB
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void m_inputBox_TextChanged(object _sender, TextChangedEventArgs _e)
        {
            try
            {
                Stat stat = Parser.Parse(m_inputBox.Text);
                m_outputBox.Text = stat?.Code ?? "";
            }
            catch (Exception err)
            {
                m_outputBox.Text = err.Message;
            }
        }
    }
}
