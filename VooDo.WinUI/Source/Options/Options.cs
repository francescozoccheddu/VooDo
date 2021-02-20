using Microsoft.UI.Xaml;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

using VooDo.Compiling;
using VooDo.Hooks;
using VooDo.Utils;
using VooDo.WinUI.Animators;
using VooDo.WinUI.Components;
using VooDo.WinUI.Core;

namespace VooDo.WinUI.Options
{

    public record BindingOptions
    {

        public static BindingOptions Empty { get; } = new BindingOptions(
            ImmutableArray.Create<Reference>(),
            ImmutableArray.Create<UsingNamespace>(),
            ImmutableArray.Create<UnresolvedType>(),
            ImmutableArray.Create<Constant>(),
            new HookInitializerList());
        public static BindingOptions Default { get; }

        static BindingOptions()
        {
            List<Reference> references = new();
            references.Add(Reference.RuntimeReference);
            references.Add(Reference.FromAssembly(Assembly.GetExecutingAssembly()));
            references.Add(Reference.FromAssembly(typeof(DependencyObject).Assembly));
            references.AddRange(Reference.GetSystemReferences());
            AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();
            references.AddRange(AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(_a => _a.GetReferencedAssemblies().Contains(assemblyName))
                .Select(_a => Reference.FromAssembly(_a)));
            IHookInitializer hookInitializer = NotifyPropertyChangedHookInitializer.Instance;
            ImmutableArray<UsingNamespace> usingNamespaces = ImmutableArray.Create<UsingNamespace>();
            ImmutableArray<UnresolvedType> usingStaticTypes = new UnresolvedType[] { typeof(AnimatorFactory) }.ToImmutableArray();
            ImmutableArray<Constant> constants = ImmutableArray.Create<Constant>();
            Default = new BindingOptions(references.ToImmutableArray(), usingNamespaces, usingStaticTypes, constants, hookInitializer);
        }

        public ImmutableArray<Reference> References { get; internal init; }
        public ImmutableArray<UsingNamespace> UsingNamespaces { get; internal init; }
        public ImmutableArray<UnresolvedType> UsingStaticTypes { get; internal init; }
        public ImmutableArray<Constant> Constants { get; internal init; }
        public IHookInitializer HookInitializer { get; internal init; }

        public BindingOptions(
            ImmutableArray<Reference> _references,
            ImmutableArray<UsingNamespace> _usingNamespaces,
            ImmutableArray<UnresolvedType> _usingStaticTypes,
            ImmutableArray<Constant> _constants,
            IHookInitializer _hookInitializer)
        {
            References = _references;
            UsingNamespaces = _usingNamespaces;
            UsingStaticTypes = _usingStaticTypes;
            Constants = _constants;
            HookInitializer = _hookInitializer;
        }

        public static BindingOptions Combine(BindingOptions _a, BindingOptions _b)
        {
            ImmutableArray<Reference> references = _a.References.AddRange(_b.References);
            ImmutableArray<UsingNamespace> usingNamespaces = _a.UsingNamespaces.AddRange(_b.UsingNamespaces);
            ImmutableArray<UnresolvedType> usingStaticTypes = _a.UsingStaticTypes.AddRange(_b.UsingStaticTypes);
            ImmutableArray<Constant> constants = _a.Constants.AddRange(_b.Constants);
            HookInitializerList hookInitializer = new HookInitializerList(_a.HookInitializer, _b.HookInitializer);
            return new BindingOptions(references, usingNamespaces, usingStaticTypes, constants, hookInitializer);
        }

    }

    public sealed record BindingManagerOptions : BindingOptions
    {
        private static BindingManagerOptions? s_userDefined;
        private static BindingManagerOptions? s_combined;

        public static new BindingManagerOptions Empty { get; }
        public static new BindingManagerOptions Default { get; }
        public static BindingManagerOptions UserDefined => s_userDefined ??= AttributeManager.RetrieveOptions();
        public static BindingManagerOptions DefaultAndUserDefined => s_combined ??= Combine(Default, UserDefined);

        public ITargetProvider TargetProvider { get; init; }

        static BindingManagerOptions()
        {
            Empty = new BindingManagerOptions(BindingOptions.Empty, new TargetProviderList());
            ITargetProvider targetProvider = new TargetProviderList(new DependencyPropertyTargetProvider());
            Default = new BindingManagerOptions(BindingOptions.Default, targetProvider);
        }

        public BindingManagerOptions(BindingOptions _options, ITargetProvider _targetProvider)
            : base(_options)
        {
            TargetProvider = _targetProvider;
        }

        public BindingManagerOptions(
            ImmutableArray<Reference> _references,
            ImmutableArray<UsingNamespace> _usingNamespaces,
            ImmutableArray<UnresolvedType> _usingStaticTypes,
            ImmutableArray<Constant> _constants,
            IHookInitializer _hookInitializer,
            ITargetProvider _targetProvider)
            : base(_references, _usingNamespaces, _usingStaticTypes, _constants, _hookInitializer)
        {
            TargetProvider = _targetProvider;
        }

        public static BindingManagerOptions Combine(BindingManagerOptions _a, BindingManagerOptions _b)
        {
            TargetProviderList targetProvider = new TargetProviderList(_a.TargetProvider, _b.TargetProvider);
            return new BindingManagerOptions(BindingOptions.Combine(_a, _b), targetProvider);
        }

    }

}
