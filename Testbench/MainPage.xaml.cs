
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;

using VooDo.Transformation;
using VooDo.Utils.Testing;

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

            }
        }

        private void CompileButton_Click(object _sender, RoutedEventArgs _e)
        {
            m_script = null;
            m_runButton.IsEnabled = false;
            try
            {
                SyntaxTree tree = CSharpSyntaxTree.ParseText(m_scriptBox.Text);
                /*BlockSyntax block = tree.GetRoot().DescendantNodes().OfType<BlockSyntax>().First();
                ScriptGenerator.Options options = ScriptGenerator.Options.Default;*/
                ScriptTransformer.Options options;
                options.HookInitializerProvider = new HookInitializerList { new TestHookInitializerProvider() };
                CompilationUnitSyntax script = ScriptTransformer.Transform((CompilationUnitSyntax) tree.GetRoot(), options);
                m_outputBlock.Text = script.NormalizeWhitespace().ToFullString();
            }
            catch (Exception exception)
            {
                m_outputBlock.Text = exception.Message;
            }
        }

        private void ScriptBox_TextChanged(object _sender, TextChangedEventArgs _e) { m_script = null; m_runButton.IsEnabled = false; }

    }
}
