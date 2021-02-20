
using System;
using System.Collections.Generic;

using VooDo.AST;
using VooDo.Caching;
using VooDo.Runtime;
using VooDo.WinUI.Interfaces;
using VooDo.WinUI.Xaml;

namespace VooDo.WinUI
{

    public sealed class BindingManager<TTarget> : IBindingManager where TTarget : ITarget
    {

        private readonly List<Binding> m_bindings;

        public BindingManager(ITargetProvider<TTarget> _targetProvider, ILoaderProvider<TTarget> _loaderProvider)
            : this(_targetProvider, _loaderProvider, new ScriptMemoryCache()) { }

        public BindingManager(ITargetProvider<TTarget> _targetProvider, ILoaderProvider<TTarget> _loaderProvider, IScriptCache _scriptCache)
        {
            m_bindings = new List<Binding>();
            Bindings = m_bindings.AsReadOnly();
            TargetProvider = _targetProvider;
            LoaderProvider = _loaderProvider;
            ScriptCache = _scriptCache;
        }

        public ITargetProvider<TTarget> TargetProvider { get; }
        public ILoaderProvider<TTarget> LoaderProvider { get; }
        public IScriptCache ScriptCache { get; }

        public IReadOnlyList<Binding> Bindings { get; }

        IEnumerable<Binding> IBindingManager.Bindings => Bindings;

        public Binding AddBinding(XamlInfo _xamlInfo)
        {
            TTarget? target = TargetProvider.GetTarget(_xamlInfo);
            if (target is null)
            {
                throw new NotSupportedException("No target could be provided for prototype");
            }
            Script script = ScriptCache.GetOrParseScript(_xamlInfo.ScriptSource);
            Program program = LoaderProvider.GetLoader(script, target).Create();
            target.AttachProgram(program);
            Binding binding = new Binding(_xamlInfo, target, program);
            m_bindings.Add(binding);
            return binding;
        }

        private static void DestroyBinding(Binding _binding)
        {
            _binding.Target.DetachProgram();
            _binding.Program.CancelRunRequest();
            foreach (Variable variable in _binding.Program.Variables)
            {
                variable.ControllerFactory = null;
            }
        }

        public void RemoveBinding(Binding _binding)
        {
            if (m_bindings.Remove(_binding))
            {
                DestroyBinding(_binding);
            }
        }

        public void ClearBindings()
        {
            foreach (Binding binding in m_bindings)
            {
                DestroyBinding(binding);
            }
            m_bindings.Clear();
        }

    }

}
