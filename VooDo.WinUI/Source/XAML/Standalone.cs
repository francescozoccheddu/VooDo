using Microsoft.UI.Xaml;

using VooDo.WinUI.Core;

namespace VooDo.WinUI.Xaml
{

    public static class Standalone
    {

        public static Binding? GetBinding(DependencyObject _obj)
        {
            return (Binding?) _obj.GetValue(BindingProperty);
        }

        public static string? GetPath(DependencyObject _obj)
        {
            return (string?) _obj.GetValue(PathProperty);
        }

        public static void SetPath(DependencyObject _obj, string? _value)
        {
            SetBinding(_value is null ? null : CodeLoader.GetCode(_value), _obj);
            _obj.SetValue(PathProperty, _value);
        }

        public static string? GetCode(DependencyObject _obj)
        {
            return (string?) _obj.GetValue(CodeProperty);
        }

        public static void SetCode(DependencyObject _obj, string? _value)
        {
            SetBinding(_value, _obj);
        }

        private static void SetBinding(string? _code, DependencyObject _object)
        {
            Binding? binding = GetBinding(_object);
            if (binding is not null)
            {
                BindingManager.RemoveBinding(binding);
            }
            _object.SetValue(CodeProperty, _code);
            if (_code is not null)
            {
                _object.SetValue(BindingProperty, binding);
                binding = BindingManager.AddBinding(XamlInfo.FromAttachedProperty(_code, _object));
                binding.Program.RequestRun();
            }
        }

#pragma warning disable IDE1006 // Naming Styles

        private static readonly DependencyProperty BindingProperty =
            DependencyProperty.RegisterAttached("Binding", typeof(Binding), typeof(Standalone), new PropertyMetadata(null));

        private static readonly DependencyProperty PathProperty =
            DependencyProperty.RegisterAttached("Path", typeof(string), typeof(Standalone), new PropertyMetadata(null));

        private static readonly DependencyProperty CodeProperty =
            DependencyProperty.RegisterAttached("Code", typeof(string), typeof(Standalone), new PropertyMetadata(null));

#pragma warning restore IDE1006 // Naming Styles

    }

}
