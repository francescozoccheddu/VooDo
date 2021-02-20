using VooDo.Runtime;
using VooDo.WinUI.Components;
using VooDo.WinUI.Xaml;

namespace VooDo.WinUI.Core
{

    public sealed class Binding
    {

        internal Binding(XamlInfo _xamlInfo, Target _target, Program _program)
        {
            XamlInfo = _xamlInfo;
            Target = _target;
            Program = _program;
        }

        public XamlInfo XamlInfo { get; }
        public Target Target { get; }
        public Program Program { get; }

    }

}
