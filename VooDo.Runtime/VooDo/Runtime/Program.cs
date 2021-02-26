﻿
using System;
using System.Collections.Generic;
using System.Linq;

using VooDo.Utils;

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
            Variables = GeneratedVariables.ToList().AsReadOnly();
            m_variableMap = Variables
                .Where(_v => _v.Name is not null)
                .GroupBy(_v => _v.Name)
                .ToDictionary(_g => _g.Key, _g => _g.ToArray());
            foreach (Variable variable in Variables)
            {
                variable.Program = this;
            }
            m_hookSets = GeneratedHooks.Select(_h => new HookSet(this, _h.hook, _h.count)).ToArray();
            Loader = null!;
        }

        public Loader Loader { get; internal set; }
        public virtual Type ReturnType => typeof(void);

        #region Hooks

        private sealed class HookSet
        {

            private static readonly IEqualityComparer<object?> s_targetComparer = new Identity.ReferenceComparer<object?>();

            private readonly HashSet<object?> m_activeTargets;
            private readonly bool[] m_subscribedInThisRun;
            private readonly IHook[] m_hooks;

            internal HookSet(IHookListener _listener, IHook _hook, int _count)
            {
                m_activeTargets = new HashSet<object?>(s_targetComparer);
                m_subscribedInThisRun = new bool[_count];
                m_hooks = new IHook[_count];
                m_hooks[0] = _hook;
                m_hooks[0].Listener = _listener;
                for (int i = 1; i < _count; i++)
                {
                    m_hooks[i] = _hook.Clone();
                    m_hooks[i].Listener = _listener;
                }
            }

            public void OnRunEnd()
            {
                for (int i = 0; i < m_subscribedInThisRun.Length; i++)
                {
                    if (!m_subscribedInThisRun[i])
                    {
                        m_hooks[i].Unsubscribe();
                    }
                    m_subscribedInThisRun[i] = false;
                }
                m_activeTargets.Clear();
            }

            public void OnSubscribe(object? _target, int _hookIndex)
            {
                if (_target is not null && !m_activeTargets.Contains(_target))
                {
                    m_subscribedInThisRun[_hookIndex] = true;
                    m_activeTargets.Add(_target);
                    m_hooks[_hookIndex].Unsubscribe();
                    m_hooks[_hookIndex].Subscribe(_target);
                }
            }

            public void UnsubscribeAll()
            {
                foreach (IHook h in m_hooks)
                {
                    h.Unsubscribe();
                }
            }

        }

        private readonly HookSet[] m_hookSets;

        protected internal TValue? SubscribeHook<TValue>(TValue? _object, int _setIndex, int _hookIndex) where TValue : class
        {
            m_hookSets[_setIndex].OnSubscribe(_object, _hookIndex);
            return _object;
        }

        public void Freeze()
        {
            foreach (HookSet s in m_hookSets)
            {
                s.UnsubscribeAll();
            }
            foreach (Variable v in Variables)
            {
                v.ControllerFactory = null;
            }
        }

        #endregion

        #region Interface

        protected internal abstract void Run();

#pragma warning disable CA1819 // Properties should not return arrays
        protected internal virtual Variable[] GeneratedVariables => Array.Empty<Variable>();
        protected internal virtual (IHook hook, int count)[] GeneratedHooks => Array.Empty<(IHook, int)>();
#pragma warning restore CA1819 // Properties should not return arrays

        #endregion

        #region Variables

        private readonly Dictionary<string, Variable[]> m_variableMap;
        public IReadOnlyList<Variable> Variables { get; }

        public IEnumerable<Variable> GetVariables(string _name)
            => m_variableMap.TryGetValue(_name, out Variable[] variables) ? variables : Enumerable.Empty<Variable>();

        public IEnumerable<Variable<TValue>> GetVariables<TValue>(string _name)
            => GetVariables(_name)
            .Where(_v => typeof(TValue).IsAssignableFrom(_v.Type))
            .Cast<Variable<TValue>>();

        public Variable? GetVariable(string _name)
            => GetVariables(_name).SingleOrDefault();

        public Variable<TValue>? GetVariable<TValue>(string _name)
            => GetVariables<TValue>(_name).SingleOrDefault();

        #endregion


        #region Run requests

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
                foreach (HookSet hookSet in m_hookSets)
                {
                    hookSet.OnRunEnd();
                }
                CancelRunRequest();
                m_running = false;
            }
        }

        private bool m_running;
        private int m_locks;
        private readonly HashSet<Locker> m_overridingLockers = new();

        public sealed class Locker : IDisposable
        {

            internal Locker(Program _program, bool _storeRequests)
            {
                Program = _program;
                StoreRequests = _storeRequests;
                if (!StoreRequests)
                {
                    Program.m_overridingLockers.Add(this);
                }
                Program.m_locks++;
            }

            public Program Program { get; }
            public bool StoreRequests { get; }
            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                if (!IsDisposed)
                {
                    IsDisposed = true;
                    Program.m_locks--;
                    if (!StoreRequests)
                    {
                        Program.m_overridingLockers.Remove(this);
                    }
                    Program.ProcessRunRequest();
                }
            }

        }

        public IDisposable Lock(bool _storeRequests = true)
            => new Locker(this, _storeRequests);

        public bool IsLocked => m_locks > 0;
        public bool IsRunRequested { get; private set; }


        private void ProcessRunRequest()
        {
            if (IsRunRequested && !IsLocked)
            {
                PrepareAndRun();
            }
        }

        public void RequestRun()
        {
            if (m_overridingLockers.Count == 0)
            {
                IsRunRequested = true;
                ProcessRunRequest();
            }
        }

        public void CancelRunRequest() => IsRunRequested = false;

        void IHookListener.NotifyChange() => RequestRun();

        #endregion

    }

}
