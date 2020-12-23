
using VooDo.AST.Statements;
using VooDo.Parsing;
using System.Linq;
using System;
using Windows.UI.Xaml.Controls;
using System.Linq.Expressions;

namespace VooDoTB
{
    public sealed partial class MainPage : Page
    {
        public MainPage() => InitializeComponent();

        private void InputBox_TextChanged(object _sender, TextChangedEventArgs _e)
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
