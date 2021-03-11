using Microsoft.UI.Xaml;

using VooDo.WinUI.Bindings;

namespace VooDo.WinUI.Xaml
{

    public static class Object
    {

        public static Binding? GetBinding(DependencyObject _obj)
            => (Binding?)_obj.GetValue(BindingProperty);

        public static void SetBinding(DependencyObject _obj, Binding? _value)
            => _obj.SetValue(BindingProperty, _value);

#pragma warning disable IDE1006 // Naming Styles

        private static readonly DependencyProperty BindingProperty =
            DependencyProperty.RegisterAttached("Binding", typeof(Binding), typeof(Object), new PropertyMetadata(null));

#pragma warning restore IDE1006 // Naming Styles

    }

}
