
using System.Collections.Generic;

namespace VooDo.Hooks
{
    public sealed class HookInitializerList : List<IHookInitializerProvider>, IHookInitializerProvider
    {

        public HookInitializerList()
        { }

        public HookInitializerList(IEnumerable<IHookInitializerProvider> _collection) : base(_collection)
        { }

        public HookInitializerList(int _capacity) : base(_capacity)
        { }


    }

}
