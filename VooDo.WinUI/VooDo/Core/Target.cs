using Microsoft.UI.Xaml;

using System;

using VooDo.WinUI.Options;

namespace VooDo.WinUI.Core
{

    public sealed partial class Target
    {

        public sealed class ReturnValueInfo
        {

            public ReturnValueInfo(UnresolvedType _type, IReturnValueSetter _setter)
            {
                Type = _type;
                Setter = _setter;
            }

            public UnresolvedType Type { get; }
            public IReturnValueSetter Setter { get; }

        }

        public interface IReturnValueSetter
        {

            void SetReturnValue(object? _value);

        }

        public Target(FrameworkElement? _owner, ReturnValueInfo? _returnValue = null)
            : this(_owner, _returnValue, BindingOptions.Empty) { }

        public Target(FrameworkElement? _owner, ReturnValueInfo? _returnValue, BindingOptions _additionalOptions)
        {
            ReturnValue = _returnValue;
            AdditionalOptions = _additionalOptions;
            Owner = _owner;
        }

        public FrameworkElement? Owner { get; }
        public BindingOptions AdditionalOptions { get; }
        public ReturnValueInfo? ReturnValue { get; }

        public Binding? Binding { get; private set; }

        internal void Bind(Binding _binding)
        {
            if (Binding is not null)
            {
                throw new InvalidOperationException("Target is already bound");
            }
            Binding = _binding;
        }

    }

}
