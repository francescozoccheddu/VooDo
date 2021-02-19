
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
        }

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
        protected internal abstract Variable[] m_Variables { get; }
        protected internal abstract IHook[] m_Hooks { get; }

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
                Run();
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

        protected internal TValue SubscribeHook<TValue>(TValue _object, int _hookIndex)
        {
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
