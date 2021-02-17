using System;

namespace VooDo.WinUI.Xaml
{

    public readonly struct XamlInfo
    {

        public object? RootObject { get; }
        public object? TargetObject { get; }
        public object? TargetProperty { get; }
        public Uri SourcePath { get; }
        public string ScriptSource { get; }

    }

}
