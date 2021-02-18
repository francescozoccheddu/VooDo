using System;

namespace VooDo.WinUI.Xaml
{

    public sealed class XamlInfo
    {

        public XamlInfo(object? _rootObject, object? _targetObject, object? _targetProperty, Uri _sourcePath, string _scriptSource)
        {
            RootObject = _rootObject;
            TargetObject = _targetObject;
            TargetProperty = _targetProperty;
            SourcePath = _sourcePath;
            ScriptSource = _scriptSource;
        }

        public object? RootObject { get; }
        public object? TargetObject { get; }
        public object? TargetProperty { get; }
        public Uri SourcePath { get; }
        public string ScriptSource { get; }

    }

}
