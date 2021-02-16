
using System.Collections.Generic;

using VooDo.AST;

namespace VooDo.WinUI
{

    public sealed class ProgramManager : IReadOnlyList<Binding>
    {

        private readonly ILoaderProvider m_loaderProvider;

        public ProgramManager(ILoaderProvider _loaderProvider)
        {
            m_loaderProvider = _loaderProvider;
        }

        public void Add(Script _script, )


    }

}
