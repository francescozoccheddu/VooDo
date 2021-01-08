using System.Collections;
using System.Collections.Generic;
using System.Linq;

using VooDo.AST;
using VooDo.Runtime.Controllers;

namespace VooDo.Runtime
{
    public sealed class Env : IReadOnlyDictionary<Name, Env.Binding>
    {

        public delegate void EvalChanged(Binding _binding, Eval _old);

        public Script Script { get; }

        private readonly IReadOnlyDictionary<Name, Binding> m_dictionary;

        public interface IBinding
        {

            event EvalChanged OnEvalChange;

            Name Name { get; }
            Env Env { get; }
            Eval Eval { get; set; }
            IController Controller { get; }
            bool HasController { get; }

            void SetController(IControllerFactory _factory);

            void NotifyEvalChange();

        }

        private sealed class SinkBinding : IBinding
        {

            public SinkBinding(Env _env, Name _name)
            {
                Name = _name;
                Env = _env;
            }

            public Name Name { get; }

            public Env Env { get; }

            public Eval Eval { get => new Eval(null); set => new Eval(null); }

            public IController Controller => null;

            public bool HasController => false;

            public event EvalChanged OnEvalChange
            {
                add { }
                remove { }
            }

            public void NotifyEvalChange() { }
            public void SetController(IControllerFactory _factory) { }
        }

        public sealed class Binding : IBinding
        {

            public event EvalChanged OnEvalChange;

            private Eval m_eval;
            private IController m_controller;

            public Name Name { get; }
            public Env Env { get; }

            internal Binding(Env _env, Name _name)
            {
                Env = _env;
                Name = _name;
                m_eval = new Eval(null);
                m_controller = null;
            }

            public Eval Eval
            {
                get
                {
                    if (HasController)
                    {
                        return m_eval = m_controller.Value;
                    }
                    else
                    {
                        return m_eval;
                    }
                }
                set
                {
                    m_eval = Eval;
                    if (HasController)
                    {
                        m_controller.Value = value;
                        NotifyEvalChange();
                    }
                    else if (value != m_eval)
                    {
                        Eval old = m_eval;
                        m_eval = value;
                        NotifyEvalChange(old);
                    }
                }
            }

            public IController Controller => m_controller;

            public bool HasController => m_controller != null;

            public void SetController(IControllerFactory _factory)
            {
                if (m_controller != null)
                {
                    m_eval = Eval;
                    m_controller.UnregisterBinding(this);
                }
                m_controller = _factory?.Create(m_controller, Eval);
            }

            private void NotifyEvalChange(Eval _old)
            {
                OnEvalChange?.Invoke(this, _old);
                Env.NotifyEvalChange(this, _old);
            }

            public void NotifyEvalChange()
            {
                Eval eval = Eval;
                if (m_eval != eval)
                {
                    Eval old = m_eval;
                    m_eval = eval;
                    NotifyEvalChange(old);
                }
            }

        }

        private void NotifyEvalChange(Binding _binding, Eval _old)
            => OnEvalChange?.Invoke(_binding, _old);

        internal Env(Script _script, IEnumerable<Name> _names)
        {
            Script = _script;
            m_dictionary = _names.ToDictionary(_n => _n, _n => new Binding(this, _n));
        }

        public event EvalChanged OnEvalChange;

        public Binding this[Name _key] => m_dictionary[_key];

        public IBinding this[Name _key, bool _sinkIfNotExists]
            => _sinkIfNotExists ? GetOrSink(_key) : this[_key];

        public IBinding GetOrSink(Name _key)
            => m_dictionary.TryGetValue(_key, out Binding value) ? value : (IBinding) new SinkBinding(this, _key);

        public int Count => m_dictionary.Count;

        public IEnumerable<Name> Keys => m_dictionary.Keys;
        public IEnumerable<Binding> Values => m_dictionary.Values;
        public bool ContainsKey(Name _key) => m_dictionary.ContainsKey(_key);
        public bool TryGetValue(Name _key, out Binding _value) => m_dictionary.TryGetValue(_key, out _value);
        public IEnumerator<KeyValuePair<Name, Binding>> GetEnumerator() => m_dictionary.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) m_dictionary).GetEnumerator();

    }

}
