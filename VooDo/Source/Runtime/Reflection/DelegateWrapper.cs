using System;

using VooDo.Runtime.Meta;
using VooDo.Utils;

namespace VooDo.Source.Runtime.Reflection
{
    public sealed class DelegateWrapper : ICallable
    {

        public DelegateWrapper(Delegate _delegate)
        {
            Ensure.NonNull(_delegate, nameof(_delegate));
            Delegate = _delegate;
        }

        public Delegate Delegate { get; }

        object ICallable.Call(object[] _arguments, Type[] _types) => Delegate.DynamicInvoke(_arguments);

        public override bool Equals(object _obj) => _obj is DelegateWrapper wrapper && Delegate.Equals(wrapper.Delegate);

        public override int GetHashCode() => Delegate.GetHashCode();

        public override string ToString() => Delegate.ToString();

    }
}
