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

        public sealed class Binding
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

        public int Count => m_dictionary.Count;

        public IEnumerable<Name> Keys => m_dictionary.Keys;
        public IEnumerable<Binding> Values => m_dictionary.Values;
        public bool ContainsKey(Name _key) => m_dictionary.ContainsKey(_key);
        public bool TryGetValue(Name _key, out Binding _value) => m_dictionary.TryGetValue(_key, out _value);
        public IEnumerator<KeyValuePair<Name, Binding>> GetEnumerator() => m_dictionary.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) m_dictionary).GetEnumerator();

    }

}
