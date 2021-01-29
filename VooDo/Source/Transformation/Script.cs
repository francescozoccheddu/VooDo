using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

using VooDo.Hooks;

namespace VooDo.Transformation
{

    public abstract class Script : IHookListener
    {

        protected Script(IEnumerable<Variable> _variables, IEnumerable<IHook> _hooks)
        {
            if (_variables == null)
            {
                throw new ArgumentNullException(nameof(_variables));
            }
            if (_hooks == null)
            {
                throw new ArgumentNullException(nameof(_hooks));
            }
            Variables = new ReadOnlyDictionary<string, Variable>(_variables.ToDictionary(_v => _v.Name, _v => _v));
            m_hooks = _hooks.ToList();
            if (m_hooks.Any(_h => _h == null))
            {
                throw new ArgumentException(nameof(_hooks), new NullReferenceException());
            }
            m_hookSubscribed = new bool[m_hooks.Count];
            foreach (IHook hook in m_hooks)
            {
                hook.Listener = this;
            }
            m_eventQueue = new Queue<EventInfo>();
        }

        public IReadOnlyDictionary<string, Variable> Variables { get; }

        private readonly IReadOnlyList<IHook> m_hooks;
        private readonly bool[] m_hookSubscribed;
        private bool m_running;
        private bool m_runRequested;
        private int m_locks;
        private readonly Queue<EventInfo> m_eventQueue;

        protected internal EventInfo m_CurrentEvent { get; private set; }

        protected internal bool IsEventFired(object _instance, int _eventIndex) => false;

        protected internal TSource SubscribeHook<TSource>(TSource _source, int _hookIndex)
        {
            if (_hookIndex > m_hooks.Count || _hookIndex < 0)
            {
                throw new ArgumentException("Bad hook index", nameof(_hookIndex), new IndexOutOfRangeException());
            }
            m_hookSubscribed[_hookIndex] = true;
            m_hooks[_hookIndex].Subscribe(_source);
            return _source;
        }

        protected internal abstract void Run();

        private void PrepareAndRun()
        {
            if (m_running)
            {
                throw new NotSupportedException("Recursion is not supported");
            }
            using (Lock())
            {
                m_running = true;
                Array.Fill(m_hookSubscribed, false);
                Run();
                for (int i = 0; i < m_hookSubscribed.Length; i++)
                {
                    if (!m_hookSubscribed[i])
                    {
                        m_hooks[i].Unsubscribe();
                    }
                }
                CancelRunRequest();
                m_running = false;
            }
        }

        private sealed class Locker : IDisposable
        {

            internal Locker(Script _script) => m_script = _script;

            private readonly Script m_script;

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
