
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Hooks;

namespace VooDo.Runtime
{

    public delegate void ProgramReturnedEventHandler<TReturn>(TReturn _value);

    public abstract class TypedProgram<TReturn> : TypedProgram
    {

        public new event ProgramReturnedEventHandler<TReturn>? OnReturn;

        protected internal abstract TReturn TypedRun();

        public sealed override Type ReturnType => typeof(TReturn);

        protected internal sealed override void Run()
        {
            TReturn value = TypedRun();
            OnReturn?.Invoke(value);
            NotifyValueReturned(value);
        }

    }

    public abstract class TypedProgram : Program
    {

        public event ProgramReturnedEventHandler<object?>? OnReturn;

        private protected void NotifyValueReturned(object? _value)
        {
            OnReturn?.Invoke(_value);
        }

    }

    public abstract class Program : IHookListener
    {

        protected Program()
        {
            Variables = m_Variables.ToImmutableArray();
            m_variableMap = Variables
                .Where(_v => _v.Name is not null)
                .GroupBy(_v => _v.Name)
                .ToImmutableDictionary(_g => _g.Key, _g => _g.ToImmutableArray());
            foreach (Variable variable in Variables)
            {
                variable.Program = this;
            }
            m_hookHolders = m_Hooks.Select(_h => new HookHolder(_h)).ToImmutableArray();
        }

        private sealed class HookHolder
        {


            private bool m_subscribedInThisRun;
            private readonly IHook m_hook;
            private object? m_lastSubscribedObject;

            internal HookHolder(IHook _hook) => m_hook = _hook;

            internal void OnRunStart()
                => m_subscribedInThisRun = false;

            internal void Subscribe(object _object)
            {
                m_subscribedInThisRun = true;
                if (!ReferenceEquals(_object, m_lastSubscribedObject))
                {
                    m_lastSubscribedObject = _object;
                    m_hook.Subscribe(_object);
                }
            }

            internal void OnRunEnd()
            {
                if (!m_subscribedInThisRun)
                {
                    m_lastSubscribedObject = null;
                    m_hook.Unsubscribe();
                }
            }

        }

        private readonly ImmutableArray<HookHolder> m_hookHolders;
        private readonly ImmutableDictionary<string, ImmutableArray<Variable>> m_variableMap;
        public ImmutableArray<Variable> Variables { get; }

        public IEnumerable<Variable> GetVariables(string _name)
            => m_variableMap.TryGetValue(_name, out ImmutableArray<Variable> variables) ? variables : Enumerable.Empty<Variable>();

        public IEnumerable<Variable<TValue>> GetVariables<TValue>(string _name)
            => GetVariables(_name)
            .Where(_v => typeof(TValue).IsAssignableFrom(_v.Type))
            .Cast<Variable<TValue>>();

        public Variable? GetVariable(string _name)
            => GetVariables(_name).SingleOrDefault();

        public Variable<TValue>? GetVariable<TValue>(string _name)
            => GetVariables<TValue>(_name).SingleOrDefault();

        private bool m_running;
        private bool m_runRequested;
        private int m_locks;

        protected internal abstract void Run();
        protected internal virtual Variable[] m_Variables => Array.Empty<Variable>();
        protected internal virtual IHook[] m_Hooks => Array.Empty<IHook>();

        public virtual Type ReturnType => typeof(void);

        private void PrepareAndRun()
        {
            if (m_running)
            {
                throw new NotSupportedException("Recursion is not supported");
            }
            using (Lock())
            {
                m_running = true;
                foreach (HookHolder holder in m_hookHolders)
                {
                    holder.OnRunStart();
                }
                Run();
                foreach (HookHolder holder in m_hookHolders)
                {
                    holder.OnRunEnd();
                }
                CancelRunRequest();
                m_running = false;
            }
        }

        private sealed class Locker : IDisposable
        {

            internal Locker(Program _script) => m_script = _script;

            private readonly Program m_script;

            private bool m_disposed;

            void IDisposable.Dispose()
            {
                if (!m_disposed)
                {
                    m_disposed = true;
                    m_script.Unlock();
                }
            }

        }

        public IDisposable Lock()
        {
            m_locks++;
            return new Locker(this);
        }

        public bool IsLocked => m_locks > 0;
        public bool IsRunRequested => m_runRequested;

        protected internal TValue? SubscribeHook<TValue>(TValue? _object, int _hookIndex) where TValue : class
        {
            if (_object is not null)
            {
                m_hookHolders[_hookIndex].Subscribe(_object);
            }
            return _object;
        }

        private void ProcessRunRequest()
        {
            if (IsRunRequested && !IsLocked)
            {
                PrepareAndRun();
            }
        }

        private void Unlock()
        {
            m_locks--;
            ProcessRunRequest();
        }

        public void RequestRun()
        {
            m_runRequested = true;
            ProcessRunRequest();
        }

        public void CancelRunRequest() => m_runRequested = false;

        void IHookListener.NotifyChange() => RequestRun();

    }

}
