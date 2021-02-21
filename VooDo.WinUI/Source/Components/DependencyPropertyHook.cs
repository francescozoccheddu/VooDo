using Microsoft.CodeAnalysis;
using Microsoft.UI.Xaml;

using System.Collections.Immutable;
using System.Reflection;

using VooDo.AST.Expressions;
using VooDo.Compiling;
using VooDo.Hooks;
using VooDo.Utils;

namespace VooDo.WinUI.Components
{

    public sealed class DependencyPropertyHookInitializer : IHookInitializer
    {

        public static DependencyPropertyHookInitializer Instance { get; } = new DependencyPropertyHookInitializer();

        private DependencyPropertyHookInitializer() { }

        public Expression? GetInitializer(ISymbol _symbol, ImmutableArray<Reference> _references)
        {
            INamedTypeSymbol? baseType = _symbol.ContainingType;
            string baseName = typeof(DependencyObject).FullName!;
            while (baseType is not null && baseType.ToDisplayString() != baseName)
            {
                baseType = baseType.BaseType;
            }
            if (baseType is null)
            {
                return null;
            }
            return new ObjectCreationExpression(
                TypeAliasResolver.Resolve(typeof(DependencyPropertyHook), _references),
                ImmutableArray.Create<Argument>(
                    new ValueArgument(null, new LiteralExpression(_symbol.Name))));
        }

    }

    public sealed class DependencyPropertyHook : IHook
    {

        private readonly string m_name;
        private DependencyObject? m_object;
        private DependencyProperty? m_property;
        private long m_token;

        public DependencyPropertyHook(string _name)
        {
            m_name = _name;
        }

        private DependencyProperty GetDependencyProperty()
        {
            if (m_property is null)
            {
                m_property = (DependencyProperty) m_object!
                    .GetType()
                    .GetProperty($"{m_name}Property", BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Static)!
                    .GetValue(null)!;
            }
            return m_property;
        }

        public IHookListener? Listener { get; set; }

        public void Subscribe(object _object)
        {
            if (!ReferenceEquals(_object, m_object))
            {
                Unsubscribe();
                m_object = (DependencyObject) _object;
                m_token = m_object.RegisterPropertyChangedCallback(GetDependencyProperty(), PropertyChanged);
            }
        }

        private void PropertyChanged(DependencyObject? _sender, DependencyProperty _property)
        {
            Listener?.NotifyChange();
        }

        public void Unsubscribe()
        {
            if (m_object is not null)
            {
                m_object.UnregisterPropertyChangedCallback(GetDependencyProperty(), m_token);
                m_object = null;
            }
        }

        public IHook Clone() => new DependencyPropertyHook(m_name);
    }

}
