using Microsoft.UI.Xaml;

using System;

using VooDo.Runtime;
using VooDo.WinUI.Xaml;

namespace VooDo.WinUI.Core
{

    public sealed class Binding
    {

        internal Binding(XamlInfo _xamlInfo, Target _target, Program _program)
        {
            XamlInfo = _xamlInfo;
            Target = _target;
            Program = _program;
            AutoAddOnLoad = true;
            Target.Bind(this);
            Lock();
            if (Target.Owner is not null)
            {
                Target.Owner.Loaded += OnLoad;
                Target.Owner.Unloaded += OnUnload;
            }
        }

        public XamlInfo XamlInfo { get; }
        public Target Target { get; }
        public Program Program { get; }

        public bool AutoAddOnLoad { get; set; }
        private IDisposable? m_lock;
        private bool m_added;

        internal void OnAdd()
        {
            if (!m_added)
            {
                m_added = true;
                if (Target.Owner?.IsLoaded ?? true)
                {
                    Unlock();
                }
            }
        }

        internal void OnRemove()
        {
            if (m_added)
            {
                m_added = false;
                Lock();
            }
        }

        private void Unlock()
        {
            if (m_lock is not null)
            {
                Program.CancelRunRequest();
                m_lock.Dispose();
                m_lock = null;
                Program.RequestRun();
            }
        }

        private void Lock()
        {
            if (m_lock is null)
            {
                m_lock = Program.Lock();
            }
        }

        private void OnLoad(object _sender, RoutedEventArgs _args)
        {
            if (m_added)
            {
                Unlock();
            }
            else if (AutoAddOnLoad)
            {
                BindingManager.AddBinding(this);
            }
        }

        private void OnUnload(object _sender, RoutedEventArgs _args)
        {
            BindingManager.RemoveBinding(this);
        }

    }

}
