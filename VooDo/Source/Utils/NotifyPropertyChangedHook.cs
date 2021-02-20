using Microsoft.CodeAnalysis;

using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;

using VooDo.AST.Expressions;
using VooDo.AST.Names;
using VooDo.Compiling;
using VooDo.Hooks;

namespace VooDo.Utils
{

    public sealed class NotifyPropertyChangedHookInitializer : IHookInitializer
    {

        public static NotifyPropertyChangedHookInitializer Instance { get; } = new();

        private NotifyPropertyChangedHookInitializer() { }

        public Expression? GetInitializer(ISymbol _symbol)
        {
            INamedTypeSymbol? type = _symbol.ContainingType;
            string? interfaceName = typeof(INotifyPropertyChanged).FullName;
            if (type is null)
            {
                return null;
            }
            if (type.AllInterfaces.Select(_i => _i.ToDisplayString()).Contains(interfaceName))
            {
                return new ObjectCreationExpression(
                    QualifiedType.FromType<NotifyPropertyChangedHook>() with { Alias = Session.runtimeReferenceAlias },
                    ImmutableArray.Create<Argument>(new ValueArgument(null, new LiteralExpression(_symbol.Name))));
            }
            return null;
        }

    }

    public sealed class NotifyPropertyChangedHook : IHook
    {

        private readonly string m_name;
        private INotifyPropertyChanged? m_object;

        public NotifyPropertyChangedHook(string _name)
        {
            m_name = _name;
        }

        public IHookListener? Listener { get; set; }

        public void Subscribe(object _object)
        {
            if (!ReferenceEquals(_object, m_object))
            {
                Unsubscribe();
                m_object = (INotifyPropertyChanged) _object;
                m_object.PropertyChanged += PropertyChanged;
            }
        }

        private void PropertyChanged(object _sender, PropertyChangedEventArgs _e)
        {
            if (_e.PropertyName == m_name)
            {
                Listener?.NotifyChange();
            }
        }

        public void Unsubscribe()
        {
            if (m_object is not null)
            {
                m_object.PropertyChanged -= PropertyChanged;
                m_object = null;
            }
        }

    }

}
