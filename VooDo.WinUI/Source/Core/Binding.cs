using VooDo.Runtime;
using VooDo.WinUI.Interfaces;
using VooDo.WinUI.Xaml;

namespace VooDo.WinUI
{

    public sealed class Binding
    {

        internal Binding(XamlInfo _xamlInfo, ITarget _target, Program _program)
        {
            XamlInfo = _xamlInfo;
            Target = _target;
            Program = _program;
        }

        public XamlInfo XamlInfo { get; }
        public ITarget Target { get; }
        public Program Program { get; }

    }

}
