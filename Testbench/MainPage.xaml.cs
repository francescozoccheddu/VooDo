
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;

using VooDo.Source.Transformation;
using VooDo.Transformation;
using VooDo.Utils;
using VooDo.Utils.Testing;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VooDoTB
{

    public sealed class Globals : DependencyObject
    {

        public string VarA
        {
            get => (string) GetValue(VarAProperty);
            set => SetValue(VarAProperty, value);
        }

        public static readonly DependencyProperty VarAProperty =
            DependencyProperty.Register("VarA", typeof(string), typeof(Globals), new PropertyMetadata(""));

        public string VarB
        {
            get => (string) GetValue(VarBProperty);
            set => SetValue(VarBProperty, value);
        }

        public static readonly DependencyProperty VarBProperty =
            DependencyProperty.Register("VarB", typeof(string), typeof(Globals), new PropertyMetadata(""));

        public string VarC
        {
            get => (string) GetValue(VarCProperty);
            set => SetValue(VarCProperty, value);
        }

        public static readonly DependencyProperty VarCProperty =
            DependencyProperty.Register("VarC", typeof(string), typeof(Globals), new PropertyMetadata(""));

    }

    public sealed partial class MainPage : Page
    {

        public MainPage() => InitializeComponent();

        public Script<Globals> Script
        {
            get => (Script<Globals>) GetValue(ScriptProperty);
            set => SetValue(ScriptProperty, value);
        }

        public static readonly DependencyProperty ScriptProperty =
            DependencyProperty.Register("Script", typeof(Script<Globals>), typeof(MainPage), new PropertyMetadata(null));


        private void RunButton_Click(object _sender, RoutedEventArgs _e)
        {
            if (Script != null)
            {
                Script.RequestRun();
            }
        }

        private void CompileButton_Click(object _sender, RoutedEventArgs _e)
        {
            Script = null;
            m_transformationBlock.Text = "";
            m_runButton.IsEnabled = false;
            string status;
            try
            {
                SyntaxTree tree = CSharpSyntaxTree.ParseText(m_sourceBox.Text);
                ScriptTransformer.Options options = ScriptTransformer.Options.Default;
                options.HookInitializerProvider = new HookInitializerList { new TestHookInitializerProvider() };
                CompilationUnitSyntax unit = ScriptTransformer.Transform((CompilationUnitSyntax) tree.GetRoot(), options);
                m_transformationBlock.Text = unit.NormalizeWhitespace().ToFullString();
                CSharpSyntaxTree transformedTree = (CSharpSyntaxTree) CSharpSyntaxTree.Create(unit);
                CSharpCompilation compilation = Compiler.Compile(transformedTree, Compiler.Options.Default);
                Script = (Script<Globals>) ScriptFactory.CreateSingle(compilation).Create();
                Script.Context = new Globals();
                m_runButton.IsEnabled = true;
                status = " Compilation succeeded";
            }
            catch (Exception exception)
            {
                status = exception.Message;
            }
            m_statusBlock.Text = $"[{DateTime.Now}] {status}";
        }

    }
}
