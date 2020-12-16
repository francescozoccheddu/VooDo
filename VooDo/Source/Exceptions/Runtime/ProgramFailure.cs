
namespace VooDo.Exceptions.Runtime
{

    public sealed class ProgramFailure : RuntimeFailure
    {

        public ProgramFailure(StatementFailure _cause) : base("Program exception", _cause) { }

        // TODO Program prop
        // TODO Cause prop

    }

}
