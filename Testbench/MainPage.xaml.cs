
using System.Diagnostics;

using VooDo.AST.Statements;
using VooDo.Parsing;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VooDoTB
{
    public sealed partial class MainPage : Page
    {
        public MainPage() => InitializeComponent();

        private void Button_Click(object _sender, RoutedEventArgs _e)
        {
            Stat stat = Parser.Parse(m_inputBox.Text);
            VooDo.Runtime.Program program = new VooDo.Runtime.Program(stat);
            program.Run();
            Debug.WriteLine(program.Environment);
        }
    }
}
