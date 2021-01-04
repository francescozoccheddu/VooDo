
using System;
using System.Linq;

using VooDo.AST.Statements;
using VooDo.Parsing;
using VooDo.Runtime.Reflection;

using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace VooDoTB
{

    public class Test
    {
        public T GetT<T>(T _t) => _t;
    }

    public sealed partial class MainPage : Page
    {
        public MainPage() => InitializeComponent();

        private void Run()
        {
            try
            {
                Stat stat = Parser.Parse(m_inputBox.Text);
                VooDo.Runtime.Program program = new VooDo.Runtime.Program(stat);
                program.Environment["System", true].Value = new TypePath("System");
                program.Environment["Test", true].Value = new Test();
                program.Run();
                m_outputBox.Text = program.Environment.FrozenDictionary.Aggregate("", (_o, _p) => $"{_o}{_p.Key}: {_p.Value} ({_p.Value?.GetType().ToString() ?? "null"})\n");
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
