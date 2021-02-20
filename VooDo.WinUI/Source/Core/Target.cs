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

        public Target(ReturnValueInfo? _returnValue = null)
            : this(_returnValue, BindingOptions.Empty) { }

        public Target(ReturnValueInfo? _returnValue, BindingOptions _additionalOptions)
        {
            ReturnValue = _returnValue;
            AdditionalOptions = _additionalOptions;
        }

        private Binding? m_binding;
        private bool m_invalidated;

        public BindingOptions AdditionalOptions { get; }
        public ReturnValueInfo? ReturnValue { get; }

        public Binding? Binding
            => m_invalidated ? null : m_binding;

        internal void Bind(Binding _binding)
        {
            if (m_binding is not null)
            {
                throw new InvalidOperationException("Target is already bound");
            }
            m_binding = _binding;
        }

        internal void Unbind()
        {
            m_invalidated = true;
        }

    }

}
