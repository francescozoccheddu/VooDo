

using System;

using VooDo.AST.Statements;

namespace VooDo.Runtime
{

    public sealed class Script
    {

        public Env Environment { get; }
        public Stat Statement { get; }
        public HookManager HookManager { get; }

        public Script(Stat _stat)
        {
            Statement = _stat;
            Environment = new Env(this, Statement.GetVariables());
            HookManager = new HookManager(this);
        }

        private bool m_runRequested;

        private int m_locks = 0;

        private void RemoveLock()
        {
            m_locks--;
            if (m_locks == 0)
            {
                if (m_runRequested)
                {
                    Run();
                }
            }
        }

        private sealed class Locker : IDisposable
        {
            private readonly Script m_script;
            private bool m_disposed = false;

            public Locker(Script _program) => m_script = _program;

            public void Dispose()
            {
                if (!m_disposed)
                {
                    m_script.m_locks--;
                    m_disposed = true;
                }
            }
        }

        public IDisposable Lock()
        {
            m_locks++;
            return new Locker(this);
        }

        public void Run()
        {
            using (Lock())
            {
                Statement.Run(Environment);
                m_runRequested = false;
            }
        }

        public void RequestRun()
        {
            m_runRequested = true;
            if (m_locks == 0)
            {
                Run();
            }
        }

    }

}
