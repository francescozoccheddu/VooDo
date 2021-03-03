
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Utils;

namespace VooDo.Runtime.Implementation
{


    public abstract class TypedProgram<TReturn> : TypedProgram, ITypedProgram<TReturn>
    {

        private event ProgramReturnedEventHandler<TReturn?>? m_OnReturn;

        #region Interface

        event ProgramReturnedEventHandler<TReturn?>? Runtime.ITypedProgram<TReturn>.OnReturn
        {
            add => m_OnReturn += value;
            remove => m_OnReturn -= value;
        }

        #endregion

        #region Internal
#pragma warning disable IDE1006 // Naming Styles

        protected internal abstract TReturn __VooDo_Reserved_TypedRun();
        private protected sealed override Type ReturnType => typeof(TReturn);

#pragma warning restore IDE1006 // Naming Styles
        #endregion

        protected sealed override void __VooDo_Reserved_Run()
        {
            TReturn value = __VooDo_Reserved_TypedRun();
            m_OnReturn?.Invoke(value);
            NotifyValueReturned(value);
        }

    }

    public abstract class TypedProgram : Program, ITypedProgram
    {

        private event ProgramReturnedEventHandler<object?>? m_OnReturn;

        #region Interface

        event ProgramReturnedEventHandler<object?>? Runtime.ITypedProgram.OnReturn
        {
            add => m_OnReturn += value;
            remove => m_OnReturn -= value;
        }

        #endregion

        private protected void NotifyValueReturned(object? _value)
        {
            m_OnReturn?.Invoke(_value);
        }

    }

    public abstract class Program : IHookListener, IProgram
    {

        protected Program()
        {
            m_variables = __VooDo_Reserved_GeneratedVariables.ToImmutableArray();
            m_variableMap = m_variables
                .Where(_v => _v.Name is not null)
                .GroupBy(_v => _v.Name)
                .ToDictionary(_g => _g.Key, _g => _g.ToImmutableArray());
            foreach (IVariable variable in m_variables)
            {
                variable.Program = this;
            }
            (IHook hook, int count)[] plainHooks = __VooDo_Reserved_GeneratedHooks;
            (__VooDo_Reserved_EventHook hook, int count)[] eventHooks = __VooDo_Reserved_GeneratedEventHooks;
            for (int i = 0; i < eventHooks.Length; i++)
            {
                eventHooks[i].hook.setIndex = i;
            }
            m_hookSets = plainHooks
                            .Concat(eventHooks.Select(_e => (hook: (IHook)_e.hook, _e.count)))
                            .Select(_h => new HookSet(this, _h.hook, _h.count))
                            .ToImmutableArray();
            loader = null!;
            m_eventQueue = new();
            m_plainHookCount = plainHooks.Length;
        }

        internal Loader loader;
        private protected virtual Type ReturnType => typeof(void);

        #region Internal
#pragma warning disable IDE1006 // Naming Styles

        protected const string __VooDo_Reserved_reservedPrefix = "__VooDo_Reserved_";
        protected const string __VooDo_Reserved_scriptPrefix = __VooDo_Reserved_reservedPrefix + "Script_";
        protected const string __VooDo_Reserved_globalPrefix = __VooDo_Reserved_reservedPrefix + "Global_";
        protected const string __VooDo_Reserved_tagPrefix = __VooDo_Reserved_reservedPrefix + "Tag_";
        protected const string __VooDo_Reserved_eventHookClassPrefix = __VooDo_Reserved_reservedPrefix + "EventHook_";

        protected static Variable<TValue> __VooDo_Reserved_CreateVariable<TValue>(bool _isConstant, string _name, TValue _value = default!) where TValue : notnull
            => new(_isConstant, _name, _value!);

        protected static TValue? __VooDo_Reserved_SetControllerAndGetValue<TValue>(Variable<TValue> _variable, IControllerFactory<TValue> _controllerFactory) where TValue : notnull
        {
            _variable.ControllerFactory = _controllerFactory;
            return _variable.Value;
        }

        protected TValue? __VooDo_Reserved_SubscribeHook<TValue>(TValue? _object, int _setIndex, int _hookIndex) where TValue : class
            => SubscribeHook(_object, _setIndex, _hookIndex);

        protected abstract void __VooDo_Reserved_Run();

        protected bool __VooDo_Reserved_SubscribeEvent(object _object, int _setIndex, int _hookIndex)
            => SubscribeEvent(_object, _setIndex, _hookIndex);

#pragma warning disable CA1819 // Properties should not return arrays
        protected virtual IVariable[] __VooDo_Reserved_GeneratedVariables => Array.Empty<IVariable>();
        protected virtual (IHook hook, int count)[] __VooDo_Reserved_GeneratedHooks => Array.Empty<(IHook, int)>();
        protected virtual (__VooDo_Reserved_EventHook hook, int count)[] __VooDo_Reserved_GeneratedEventHooks => Array.Empty<(__VooDo_Reserved_EventHook, int)>();
#pragma warning restore CA1819 // Properties should not return arrays

#pragma warning restore IDE1006 // Naming Styles
        #endregion

        #region Interface

        Loader IProgram.Loader => loader;
        Type IProgram.ReturnType => ReturnType;
        void IProgram.Freeze() => Freeze();
        ImmutableArray<IVariable> IProgram.Variables { get; }
        bool IProgram.IsRunRequested => m_isRunRequested;
        bool IProgram.IsLocked => m_IsLocked;
        bool IProgram.IsStoringRequests => m_IsStoringRequests;
        ILocker IProgram.Lock(bool _storeRequests) => Lock(_storeRequests);
        void IProgram.RequestRun() => RequestRun();
        void IProgram.CancelRunRequest() => CancelRunRequest();
        void IHookListener.NotifyChange() => NotifyChange();
        ImmutableArray<IVariable> IProgram.GetVariables(string _name) => GetVariables(_name);
        IEnumerable<Variable<TValue>> IProgram.GetVariables<TValue>(string _name) => GetVariables<TValue>(_name);
        IVariable? IProgram.GetVariable(string _name) => GetVariable(_name);
        Variable<TValue>? IProgram.GetVariable<TValue>(string _name) => GetVariable<TValue>(_name);

        #endregion

        #region Event hooks

        private readonly Queue<(object obj, int set)> m_eventQueue;
        private readonly int m_plainHookCount;

#pragma warning disable IDE1006 // Naming Styles
        protected abstract class __VooDo_Reserved_EventHook : IHook
#pragma warning restore IDE1006 // Naming Styles
        {

            private Program? m_program;
            private object? m_target;
            internal int setIndex;

            IHookListener? IHook.Listener { set => m_program = (Program?)value; }

            IHook IHook.Clone() => (IHook)MemberwiseClone();

            void IHook.Subscribe(object _object)
            {
                ((IHook)this).Unsubscribe();
                __VooDo_Reserved_SetSubscribed(_object, true);
                m_target = _object;
            }

            void IHook.Unsubscribe()
            {
                if (m_target is not null)
                {
                    __VooDo_Reserved_SetSubscribed(m_target, false);
                    m_target = null;
                }
            }

            private void Notify()
            {
                if (m_program is not null)
                {
                    m_program.m_eventQueue.Enqueue((m_target!, setIndex));
                    m_program.ProcessRunRequest();
                }
            }

#pragma warning disable IDE1006 // Naming Styles

            protected const string __VooDo_Reserved_onEventMethodName = __VooDo_Reserved_reservedPrefix + "OnEvent";

            protected void __VooDo_Reserved_Notify()
                => Notify();

            protected abstract void __VooDo_Reserved_SetSubscribed(object _object, bool _subscribed);

#pragma warning restore IDE1006 // Naming Styles

        }

        private bool SubscribeEvent(object _object, int _setIndex, int _hookIndex)
        {
            m_hookSets[_setIndex + m_plainHookCount].OnSubscribe(_object, _hookIndex);
            if (m_eventQueue.Count > 0)
            {
                (object qObject, int qSet) = m_eventQueue.Peek();
                return qSet == _setIndex && ReferenceEquals(qObject, _object);
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Hooks

        private sealed class HookSet
        {

            private readonly HashSet<object> m_activeTargets;
            private readonly bool[] m_subscribedInThisRun;
            private readonly IHook[] m_hooks;

            internal HookSet(IHookListener _listener, IHook _hook, int _count)
            {
                m_activeTargets = new(Identity.ReferenceComparer<object>.Instance);
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

            internal void OnRunEnd()
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

            internal void OnSubscribe(object? _target, int _hookIndex)
            {
                if (_target is not null && !m_activeTargets.Contains(_target))
                {
                    m_subscribedInThisRun[_hookIndex] = true;
                    m_activeTargets.Add(_target);
                    m_hooks[_hookIndex].Unsubscribe();
                    m_hooks[_hookIndex].Subscribe(_target);
                }
            }

            internal void UnsubscribeAll()
            {
                foreach (IHook h in m_hooks)
                {
                    h.Unsubscribe();
                }
            }

        }

        private readonly ImmutableArray<HookSet> m_hookSets;

        private void Freeze()
        {
            foreach (HookSet s in m_hookSets)
            {
                s.UnsubscribeAll();
            }
            m_eventQueue.Clear();
            foreach (IVariable v in m_variables)
            {
                v.Freeze();
            }
        }

        private TValue? SubscribeHook<TValue>(TValue? _object, int _setIndex, int _hookIndex) where TValue : class
        {
            m_hookSets[_setIndex].OnSubscribe(_object, _hookIndex);
            return _object;
        }

        #endregion

        #region Variables

        private readonly Dictionary<string, ImmutableArray<IVariable>> m_variableMap;
        private readonly ImmutableArray<IVariable> m_variables;

        private ImmutableArray<IVariable> GetVariables(string _name)
            => m_variableMap.TryGetValue(_name, out ImmutableArray<IVariable> variables) ? variables : ImmutableArray.Create<IVariable>();

        private IEnumerable<Variable<TValue>> GetVariables<TValue>(string _name) where TValue : notnull
            => GetVariables(_name)
            .Where(_v => typeof(TValue).IsAssignableFrom(_v.Type))
            .Cast<Variable<TValue>>();

        private IVariable? GetVariable(string _name)
            => GetVariables(_name).SingleOrDefault();

        private Variable<TValue>? GetVariable<TValue>(string _name) where TValue : notnull
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
                do
                {
                    __VooDo_Reserved_Run();
                    foreach (HookSet hookSet in m_hookSets)
                    {
                        hookSet.OnRunEnd();
                    }
                    if (m_eventQueue.Count > 0)
                    {
                        m_eventQueue.Dequeue();
                    }
                }
                while (m_eventQueue.Count > 0);
                CancelRunRequest();
                m_running = false;
            }
        }

        private bool m_isRunRequested;
        private bool m_running;
        private int m_locks;
        private readonly HashSet<Locker> m_overridingLockers = new();
        private bool m_IsStoringRequests => m_overridingLockers.Count == 0;
        private bool m_IsLocked => m_locks > 0;

        private sealed class Locker : ILocker
        {

            internal Locker(Program _program, bool _storeRequests)
            {
                m_program = _program;
                m_storeRequests = _storeRequests;
                if (!m_storeRequests)
                {
                    m_program.m_overridingLockers.Add(this);
                }
                m_program.m_locks++;
            }

            #region Interface

            IProgram ILocker.Program => m_program;
            bool ILocker.IsDisposed => m_isDisposed;
            bool ILocker.AllowsStoringRequests => m_isDisposed;
            void IDisposable.Dispose() => Dispose();

            #endregion

            private readonly Program m_program;
            private readonly bool m_storeRequests;
            private bool m_isDisposed;

            private void Dispose()
            {
                if (!m_isDisposed)
                {
                    m_isDisposed = true;
                    m_program.m_locks--;
                    if (!m_storeRequests)
                    {
                        m_program.m_overridingLockers.Remove(this);
                    }
                    m_program.ProcessRunRequest();
                }
            }

        }

        private Locker Lock(bool _storeRequests = true)
            => new(this, _storeRequests);

        private void ProcessRunRequest()
        {
            if ((m_isRunRequested || m_eventQueue.Count > 0) && !m_IsLocked)
            {
                PrepareAndRun();
            }
        }

        private void RequestRun()
        {
            if (!m_IsLocked || m_IsStoringRequests)
            {
                m_isRunRequested = true;
                ProcessRunRequest();
            }
        }

        private void CancelRunRequest() => m_isRunRequested = false;

        private void NotifyChange() => RequestRun();

        #endregion

    }


}
