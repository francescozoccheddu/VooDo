
using System;
using System.Linq;

using VooDo.AST.Statements;
using VooDo.Parsing;
using VooDo.Runtime;
using VooDo.Runtime.Reflection;

using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace VooDoTB
{

    public sealed partial class MainPage : Page
    {
        public MainPage() => InitializeComponent();

        private void Run()
        {
            try
            {
                Stat stat = Parser.Parse(m_inputBox.Text);
                Script program = new Script(stat);
                try
                {
                    program.Environment["System"].Eval = new Eval(new TypePath("System"));
                }
                catch { }
                program.Run();
                m_outputBox.Text = program.Environment.Aggregate("", (_o, _p) => $"{_o}{_p.Key}: {_p.Value.Eval}\n");
            }
            catch (Exception err)
            {
                m_outputBox.Text = err.Message;
            }
        }

        private void InputBox_PreviewKeyDown(object _sender, KeyRoutedEventArgs _e)
        {
            if (Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down) && _e.Key == VirtualKey.Enter)
            {
                _e.Handled = true;
                Run();
            }
        }
    }
}
