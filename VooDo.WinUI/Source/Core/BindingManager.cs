
using System;
using System.Collections.Generic;
using System.Linq;

using VooDo.AST;
using VooDo.AST.Directives;
using VooDo.AST.Names;
using VooDo.Caching;
using VooDo.Compiling.Emission;
using VooDo.Runtime;
using VooDo.Utils;
using VooDo.WinUI.Options;
using VooDo.WinUI.Xaml;

namespace VooDo.WinUI.Core
{

    public static class BindingManager
    {

        private static readonly HashSet<Binding> s_bindings = new(new Identity.ReferenceComparer<Binding>());
        private static readonly IScriptCache s_scriptCache = new ScriptMemoryCache();
        private static readonly ILoaderCache s_loaderCache = new LoaderMemoryCache();

        public static IReadOnlySet<Binding> Bindings { get; } = s_bindings;

        private static Script ProcessScript(Script _script, BindingOptions _options)
        {
            _script = _script.AddGlobals(_options.Constants.Select(_c => new Global(true, _c.Type.Resolve(_options.References), _c.Name)));
            _script = _script.AddUsingStaticTypes(_options.UsingStaticTypes.Select(_t => (QualifiedType) _t.Resolve(_options.References)));
            _script = _script.AddUsingDirectives(_options.UsingNamespaces.Select(_u => new UsingNamespaceDirective(_u.Alias, _u.Namespace)));
            return _script;
        }

        public static void AddBinding(Binding _binding)
        {
            if (s_bindings.Add(_binding))
            {
                _binding.OnAdd();
            }
        }

        public static Binding CreateBinding(XamlInfo _xamlInfo)
        {
            Target? target = BindingManagerOptions.DefaultAndUserDefined.TargetProvider.GetTarget(_xamlInfo);
            if (target is null)
            {
                throw new NotSupportedException("No target could be provided");
            }
            Script script = s_scriptCache.GetOrParseScript(_xamlInfo.Script);
            BindingOptions options = BindingOptions.Combine(target.AdditionalOptions, BindingManagerOptions.DefaultAndUserDefined);
            script = ProcessScript(script, options);
            ComplexType? returnType = target.ReturnValue?.Type.Resolve(options.References);
            LoaderKey loaderKey = LoaderKey.Create(script, options.References, returnType, options.HookInitializer);
            Program program = s_loaderCache.GetOrCreateLoader(loaderKey).Create();
            using (program.Lock())
            {
                foreach (Constant constant in options.Constants)
                {
                    program.GetVariable(constant.Name)!.Value = constant.Value;
                }
                program.CancelRunRequest();
            }
            if (target.ReturnValue is not null)
            {
                ((TypedProgram) program).OnReturn += target.ReturnValue.Setter.SetReturnValue;
            }
            return new Binding(_xamlInfo, target, program);
        }


        public static void RemoveBinding(Binding _binding)
        {
            if (s_bindings.Remove(_binding))
            {
                _binding.OnRemove();
            }
        }

        public static void ClearBindings()
        {
            foreach (Binding binding in s_bindings)
            {
                binding.OnRemove();
            }
            s_bindings.Clear();
        }

    }

}
