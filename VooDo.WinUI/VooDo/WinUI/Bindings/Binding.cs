using System;
using System.Reflection;

using VooDo.Runtime;
using VooDo.WinUI.Generator;
using VooDo.WinUI.Utils;

namespace VooDo.WinUI.Bindings
{

    public abstract class Binding
    {

        internal Binding(IProgram _program, object _xamlOwner, object _xamlRoot, string _sourcePath, string _sourceTag)
        {
            Program = _program;
            XamlOwner = _xamlOwner;
            XamlRoot = _xamlRoot;
            SourceTag = _sourceTag;
            SourcePath = _sourcePath;
            Lock();
        }

        public virtual IProgram Program { get; }
        public object XamlOwner { get; }
        public object XamlRoot { get; }
        public string SourceTag { get; }
        public string SourcePath { get; }

        public enum ETarget
        {
            Class, Property
        }

        public abstract ETarget Target { get; }

        private IDisposable? m_lock;

        private void Lock()
        {
            if (m_lock is null)
            {
                m_lock = Program.Lock(false);
                Program.Freeze();
            }
        }

        private void Unlock()
        {
            if (m_lock is not null)
            {
                m_lock.Dispose();
                m_lock = null;
                Program.RequestRun();
            }
        }

        internal void OnAdd()
        {
            Unlock();
        }

        internal void OnRemove()
        {
            Lock();
        }

    }

    public sealed class ClassBinding : Binding
    {

        public override ETarget Target => ETarget.Class;

        internal ClassBinding(IProgram _program, object _xamlOwner)
            : base(_program, _xamlOwner, _xamlOwner, _program.Loader.GetStringTag(Identifiers.classSourceTag), _program.Loader.GetStringTag(Identifiers.classTagTag))
        {
            Program.GetVariable(Identifiers.classThisVariableName)!.Value = _xamlOwner;
        }

    }

    public sealed class PropertyBinding : Binding
    {

        public override ETarget Target => ETarget.Property;
        public MemberInfo Property { get; }
        public override ITypedProgram Program => (ITypedProgram)base.Program;

        private PropertyBinding(ITypedProgram _program, MemberInfo _property, object _xamlOwner, object _xamlRoot, string _tag, string _xamlPath, DynamicSetterHelper.Setter _setter)
            : base(_program, _xamlOwner, _xamlRoot, _xamlPath, _tag)
        {
            Property = _property;
            Program.GetVariable(Identifiers.propertyThisVariableName)!.Value = _xamlOwner;
            Program.GetVariable(Identifiers.propertyRootVariableName)!.Value = _xamlRoot;
            Program.OnReturn += _o => _setter(_o);
        }

        internal PropertyBinding(ITypedProgram _program, PropertyInfo _property, object _xamlOwner, object _xamlRoot, string _xamlPath, string _tag)
            : this(_program, _property, _xamlOwner, _xamlRoot, _xamlPath, _tag, DynamicSetterHelper.GetSetter(_property, _xamlOwner))
        { }

        internal PropertyBinding(ITypedProgram _program, FieldInfo _property, object _xamlOwner, object _xamlRoot, string _xamlPath, string _tag)
            : this(_program, _property, _xamlOwner, _xamlRoot, _xamlPath, _tag, DynamicSetterHelper.GetSetter(_property, _xamlOwner))
        { }

    }

}
