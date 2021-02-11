
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using VooDo.Hooks;

namespace VooDo.Runtime
{

    public abstract class Program<TReturn> : Program
    {

        protected Program(IEnumerable<Variable> _variables) : base(_variables)
        {
        }

        protected internal abstract TReturn TypedRun();

        protected internal sealed override void Run()
            => throw new NotImplementedException();

    }

    public abstract class Program : IHookListener
    {

        protected Program(IEnumerable<Variable> _variables)
        {
            Variables = new ReadOnlyDictionary<string, Variable>(_variables.ToDictionary(_v => _v.Name, _v => _v));
        }

        public IReadOnlyDictionary<string, Variable> Variables { get; }

        private bool m_running;
        private bool m_runRequested;
        private int m_locks;

        protected internal abstract void Run();
        protected internal abstract IEnumerable<Variable> m_Variables { get; }


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
