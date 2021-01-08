
using VooDo.AST;

namespace VooDo.Runtime.Hooks.Common
{

    public sealed class EnvHookProvider : TypedHookProvider<Env>
    {

        private sealed class Hook : IHook
        {

            internal Hook(Env.Binding _binding)
            {
                m_binding = _binding;
                m_binding.OnEvalChange += NotifyEvalChange;
            }

            private void NotifyEvalChange(Env.Binding _binding, Eval _old) => OnChange?.Invoke();

            private readonly Env.Binding m_binding;

            public event HookEventHandler OnChange;

            public void Unsubscribe() => m_binding.OnEvalChange -= NotifyEvalChange;

        }

        protected override IHook Subscribe(Env _instance, Name _property) => new Hook(_instance[_property]);
    }

}
