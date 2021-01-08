
using System;
using System.Linq;

using VooDo.AST.Statements;
using VooDo.Parsing;
using VooDo.Runtime;
using VooDo.Runtime.Reflection;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VooDoTB
{

    public sealed class Test : DependencyObject
    {

        public string Var
        {
            get => (string) GetValue(VarProperty);
            set => SetValue(VarProperty, value);
        }

        public static readonly DependencyProperty VarProperty =
            DependencyProperty.Register("Var", typeof(string), typeof(Test), new PropertyMetadata(""));

    }

    public sealed partial class MainPage : Page
    {

        private readonly Test m_test = new Test();

        public MainPage() => InitializeComponent();

        private Script m_script = null;

        private void VarBox_TextChanged(object _sender, TextChangedEventArgs _e) => m_test.Var = m_varBox.Text;

        private void RunButton_Click(object _sender, RoutedEventArgs _e)
        {
            if (m_script != null)
            {
                try
                {
                    m_script.Run();
                }
                catch (Exception error)
                {
                    SetError(error);
                }
            }
        }

        private void CompileButton_Click(object _sender, RoutedEventArgs _e)
        {
            m_script = null;
            m_runButton.IsEnabled = false;
            try
            {
                Stat stat = Parser.Parse(m_scriptBox.Text);
                m_script = new Script(stat);
                m_script.Environment.OnEvalChange += (_b, _o) => UpdateEnv();
                m_runButton.IsEnabled = true;
                m_script.Environment["System", true].Eval = new Eval(new TypePath("System"));
                m_script.Environment["var", true].Eval = new Eval(m_test);
            }
            catch (Exception error)
            {
                SetError(error);
            }
        }

        private void SetError(Exception _error) => m_envGrid.ItemsSource = new Binding[] { new Binding(_error) };

        private void UpdateEnv()
        {
            if (m_script != null)
            {
                m_envGrid.ItemsSource = m_script.Environment.Values.Select(_b => new Binding(_b));
            }
        }

        private void ScriptBox_TextChanged(object _sender, TextChangedEventArgs _e) { m_script = null; m_runButton.IsEnabled = false; }

    }
}
