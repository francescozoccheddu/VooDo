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
            string? code = _value is null ? null : CodeLoader.GetCode(_value);
            _obj.SetValue(PathProperty, _value);
            _obj.SetValue(CodeProperty, code);
            UpdateBinding(_obj);
        }

        public static string? GetCode(DependencyObject _obj)
        {
            return (string?) _obj.GetValue(CodeProperty);
        }

        public static void SetCode(DependencyObject _obj, string? _value)
        {
            _obj.SetValue(PathProperty, null);
            _obj.SetValue(CodeProperty, _value);
            UpdateBinding(_obj);
        }

        private static void UpdateBinding(DependencyObject _obj)
        {
            Binding? binding = GetBinding(_obj);
            string? code = GetCode(_obj);
            if (binding is not null)
            {
                binding.AutoAddOnLoad = false;
                BindingManager.RemoveBinding(binding);
                binding = null;
            }
            if (code is not null)
            {
                binding = BindingManager.CreateBinding(XamlInfo.FromAttachedProperty(code, _obj));
                BindingManager.AddBinding(binding);
            }
            _obj.SetValue(BindingProperty, binding);
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
