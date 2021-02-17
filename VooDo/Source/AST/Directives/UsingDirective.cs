namespace VooDo.AST.Directives
{

    public abstract record UsingDirective : BodyNode
    {

#if NET5_0
        public sealed override Script? Parent => (Script?) base.Parent;
#endif

    }

}
