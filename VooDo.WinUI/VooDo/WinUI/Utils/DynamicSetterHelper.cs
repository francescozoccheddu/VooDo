using System.Reflection;

namespace VooDo.WinUI.Utils
{

    internal static class DynamicSetterHelper
    {

        private static readonly LRUCache<(MemberInfo, object?), Setter> s_cache = new(256);

        internal delegate void Setter(object? _value);

        private static Setter CreateSetter(FieldInfo _field, object? _object)
            => _v => _field.SetValue(_object, _v);

        private static Setter CreateSetter(PropertyInfo _property, object? _object)
            => _v => _property.SetValue(_object, _v);

        internal static Setter GetSetter(FieldInfo _field, object? _object)
        {
            if (!s_cache.TryGetValue((_field, _object), out Setter setter))
            {
                setter = CreateSetter(_field, _object);
            }
            return setter;
        }

        internal static Setter GetSetter(PropertyInfo _property, object? _object)
        {
            if (!s_cache.TryGetValue((_property, _object), out Setter setter))
            {
                setter = CreateSetter(_property, _object);
            }
            return setter;
        }

    }

}
