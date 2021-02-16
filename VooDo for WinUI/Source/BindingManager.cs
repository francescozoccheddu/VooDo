
using System;
using System.Collections.Generic;

using VooDo.AST;
using VooDo.Caching;
using VooDo.Runtime;

namespace VooDo.WinUI
{

    public sealed class BindingManager
    {

        public sealed class Binding
        {

            internal Binding(Script _source, Target _target, Program _program)
            {
                Source = _source;
                Target = _target;
                Program = _program;
            }

            public Script Source { get; }
            public Target Target { get; }
            public Program Program { get; }

        }

        private readonly List<Binding> m_bindings;

        public BindingManager(ITargetProvider _targetProvider, ILoaderProvider _loaderProvider)
            : this(_targetProvider, _loaderProvider, new ScriptMemoryCache()) { }

        public BindingManager(ITargetProvider _targetProvider, ILoaderProvider _loaderProvider, IScriptCache _scriptCache)
        {
            m_bindings = new List<Binding>();
            Bindings = m_bindings.AsReadOnly();
            TargetProvider = _targetProvider;
            LoaderProvider = _loaderProvider;
            ScriptCache = _scriptCache;
        }

        public ITargetProvider TargetProvider { get; }
        public ILoaderProvider LoaderProvider { get; }
        public IScriptCache ScriptCache { get; }

        public IReadOnlyList<Binding> Bindings { get; }

        public Binding AddBinding(string _source, object _targetPrototype)
        {
            Target? target = TargetProvider.GetTarget(_targetPrototype);
            if (target is null)
            {
                throw new NotSupportedException("No target could be provided for prototype");
            }
            Script script = ScriptCache.GetOrParseScript(_source);
            Program program = LoaderProvider.GetLoader(script, target).Create();
            Binding binding = new Binding(script, target, program);
            m_bindings.Add(binding);
            return binding;
        }

        public void RemoveBinding(Binding _binding)
        {
            if (m_bindings.Remove(_binding))
            {
                _binding.Target.DetachProgram();
                _binding.Program.CancelRunRequest();
                foreach (Variable variable in _binding.Program.Variables)
                {
                    variable.ControllerFactory = null;
                }
            }
        }

    }

}
