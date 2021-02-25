using System.ComponentModel;

using VooDo.Runtime;

namespace VooDo.WinUI.Hooks
{

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
