using VooDo.Runtime;

namespace VooDo.Caching
{

    public interface ILoaderCache
    {

        Loader GetOrCreateLoader(LoaderKey _key);

    }

}
