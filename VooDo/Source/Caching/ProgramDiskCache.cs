using System;

using VooDo.AST;
using VooDo.Compiling;
using VooDo.Runtime;

namespace VooDo.Caching
{

    public sealed class ProgramDiskCache : IProgramCache
    {

        public static void Clear(string _path)
        {

        }

        public ProgramDiskCache(string _path)
        {

        }

        public int Count { get; }


        public Loader GetOrCreate(Script _script, CompilationOptions _options)
        {
            throw new NotImplementedException();
        }

        public void Reload()
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        Loader? IProgramCache.GetLoader(ProgramKey _key)
            => throw new NotImplementedException();

    }

}
