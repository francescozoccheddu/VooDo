using Microsoft.UI.Xaml.Markup;

using System;

namespace VooDo.WinUI.Xaml
{

    public sealed class XamlInfo
    {

        public XamlInfo(string _script, ProvideValueTargetProperty _property, object? _object, object? _root, Uri _path)
        {
            Script = _script;
            Property = _property;
            Object = _object;
            Root = _root;
            Path = _path;
        }

        public string Script { get; }
        public ProvideValueTargetProperty Property { get; }
        public object? Object { get; }
        public object? Root { get; }
        public Uri Path { get; }

    }

}
