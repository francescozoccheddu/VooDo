
using VooDo.Compiling;
using VooDo.Runtime;

namespace VooDo.Caching
{

    public interface IProgramCache
    {

        Loader Cache(Compilation _compilation);

        Loader? GetLoader(ProgramKey _key);

    }

}
