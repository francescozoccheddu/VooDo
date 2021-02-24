using Microsoft.CodeAnalysis;

using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;

using VooDo.AST.Expressions;
using VooDo.AST.Names;
using VooDo.Compiling;
using VooDo.Hooks;
using VooDo.Runtime;

namespace VooDo.Utils
{

    public sealed class NotifyPropertyChangedHookInitializer : IHookInitializer
    {

        public static NotifyPropertyChangedHookInitializer Instance { get; } = new();

        private NotifyPropertyChangedHookInitializer() { }

        public Expression? GetInitializer(ISymbol _symbol, ImmutableArray<Reference> _references)
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

    public sealed class NotifyPropertyChangedHook : Hook<INotifyPropertyChanged, INotifyPropertyChanged>
    {

        private readonly string m_name;
        public NotifyPropertyChangedHook(string _name) => m_name = _name;

        private void PropertyChanged(object? _sender, PropertyChangedEventArgs _e)
        {
            if (_e.PropertyName == m_name)
            {
                NotifyChange();
            }
        }

        public override IHook Clone() => new NotifyPropertyChangedHook(m_name);
        protected override INotifyPropertyChanged Subscribe(INotifyPropertyChanged _object)
        {
            _object.PropertyChanged += PropertyChanged;
            return _object;
        }

        protected override void Unsubscribe(INotifyPropertyChanged _object) => _object.PropertyChanged -= PropertyChanged;

    }

}
