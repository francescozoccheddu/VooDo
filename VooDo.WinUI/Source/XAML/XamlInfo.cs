using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;

using System;

namespace VooDo.WinUI.Xaml
{

    public sealed class XamlInfo
    {

        public enum ESourceKind
        {
            MarkupExtension, AttachedProperty
        }

        internal static XamlInfo FromAttachedProperty(string _script, DependencyObject _object)
            => new XamlInfo(_script, _object);

        internal static XamlInfo FromMarkupExtension(string _script, ProvideValueTargetProperty _property, object? _object, object? _root, Uri _path)
            => new XamlInfo(_script, _property, _object, _root, _path);

        private XamlInfo(string _script, DependencyObject _object)
        {
            SourceKind = ESourceKind.AttachedProperty;
            Script = _script;
            Object = _object;
        }

        private XamlInfo(string _script, ProvideValueTargetProperty _property, object? _object, object? _root, Uri _path)
        {
            SourceKind = ESourceKind.MarkupExtension;
            Script = _script;
            Property = _property;
            Object = _object;
            Root = _root;
            Path = _path;
        }

        public ESourceKind SourceKind { get; }
        public string Script { get; }
        public ProvideValueTargetProperty? Property { get; }
        public object? Object { get; }
        public object? Root { get; }
        public Uri? Path { get; }

    }

}
