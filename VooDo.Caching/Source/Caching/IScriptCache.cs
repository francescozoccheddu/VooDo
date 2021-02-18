
using VooDo.AST;

namespace VooDo.Caching
{

    public interface IScriptCache
    {

        Script GetOrParseScript(string _source);

    }

}
