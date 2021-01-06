using System;
using System.Collections.Generic;
using System.Linq;

using VooDo.AST;
using VooDo.Source.Runtime;
using VooDo.Utils;

namespace VooDo.Runtime
{
    public sealed class Env
    {

        public delegate void ValueChanged(Binding _binding, object _oldValue);
        public delegate void BindingCreated(Binding _binding);

        public sealed class Binding
        {

            private object m_data;
            private object m_oldValue;

            internal Binding(Env _environment, Name _name)
            {
                Environment = _environment;
                Name = _name;
                m_data = null;
                HasController = false;
            }

            public event ValueChanged OnValueChanged;

            public Name Name { get; }
            public Env Environment { get; }

            internal void NotifyValueChanged()
            {
                object value = Value;
                if (value != m_oldValue)
                {
                    OnValueChanged?.Invoke(this, m_oldValue);
                    Environment.NotifyValueChanged(this, m_oldValue);
                    m_oldValue = value;
                }
            }

            public IController Controller
            {
                get => HasController ? (IController) m_data : null;
                set
                {
                    if (value == null)
                    {
                        m_data = value;
                        HasController = false;
                    }
                    else
                    {
                        m_data = value;
                        HasController = true;
                        NotifyValueChanged();
                    }
                }
            }

            public object Value
            {
                get => HasController ? Controller.Value : m_data;
                set
                {
                    if (HasController)
                    {
                        Controller.Value = value;
                    }
                    else
                    {
                        m_data = value;
                        NotifyValueChanged();
                    }
                }
            }

            public bool HasController { get; private set; }

            public void UpdateController(IControllerFactory _factory)
            {
                Ensure.NonNull(_factory, nameof(_factory));
                Controller = _factory.Create(Controller, Value);
            }


        }

        private readonly Dictionary<Name, Binding> m_dictionary = new Dictionary<Name, Binding>();

        internal Env(Script _script)
        {
            Ensure.NonNull(_script, nameof(_script));
            Script = _script;
        }

        private void NotifyValueChanged(Binding _binding, object _oldValue) => OnValueChanged?.Invoke(_binding, _oldValue);

        public event ValueChanged OnValueChanged;
        public event BindingCreated OnBindingCreated;

        public Script Script { get; }

        public Binding Add(Name _name)
        {
            Binding binding = new Binding(this, _name);
            m_dictionary.Add(_name, binding);
            OnBindingCreated?.Invoke(binding);
            return binding;
        }

        public bool TryAdd(Name _name, out Binding _binding)
        {
            if (ContainsName(_name))
            {
                _binding = null;
                return false;
            }
            else
            {
                _binding = Add(_name);
                return true;
            }
        }

        public bool ContainsName(Name _name) => m_dictionary.ContainsKey(_name);

        public bool TryGet(Name _name, out Binding _binding) => m_dictionary.TryGetValue(_name, out _binding);

        public IReadOnlyCollection<Name> Names => m_dictionary.Keys;

        public IReadOnlyCollection<Binding> Bindings => m_dictionary.Values;

        public Dictionary<Name, object> FrozenDictionary => m_dictionary.ToDictionary(_e => _e.Key, _e => _e.Value.Value);

        public enum EInjectReplaceMode
        {
            Replace,
            Skip,
            Throw
        }

        public void Inject(IEnumerable<KeyValuePair<Name, object>> _values, EInjectReplaceMode _mode = EInjectReplaceMode.Throw)
        {
            switch (_mode)
            {
                case EInjectReplaceMode.Replace:
                foreach (KeyValuePair<Name, object> entry in _values)
                {
                    Binding binding = this[entry.Key, true];
                    binding.Controller = null;
                    binding.Value = entry.Value;
                }
                break;
                case EInjectReplaceMode.Skip:
                foreach (KeyValuePair<Name, object> entry in _values)
                {
                    if (TryAdd(entry.Key, out Binding binding))
                    {
                        binding.Value = entry.Value;
                    }
                }
                break;
                case EInjectReplaceMode.Throw:
                foreach (KeyValuePair<Name, object> entry in _values)
                {
                    Add(entry.Key).Value = entry.Value;
                }
                break;
            }
        }

        public Binding this[Name _name, bool _createIfNotExists = false]
        {
            get
            {
                if (TryGet(_name, out Binding binding))
                {
                    return binding;
                }
                else if (_createIfNotExists)
                {
                    return Add(_name);
                }
                else
                {
                    throw new Exception("Missing key");
                }
            }
        }

    }

}
