using System;
using System.Collections.Generic;

namespace VooDo.Runtime
{

    public sealed class Loader
    {

        public static Loader FromCodeFile(string _file)
            => throw new NotImplementedException();

        public static Loader FromLibraryFile(string _file)
            => throw new NotImplementedException();

        public static Loader FromMemory(IEnumerable<byte> _file)
            => throw new NotImplementedException();

        public Program Create()
            => throw new NotImplementedException();

        public Program<TReturn> Create<TReturn>()
            => throw new NotImplementedException();

    }

}
