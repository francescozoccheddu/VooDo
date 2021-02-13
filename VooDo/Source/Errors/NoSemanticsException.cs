namespace VooDo.Errors
{

    public class NoSemanticsException : VooDoException
    {

        internal NoSemanticsException() : base("Unable to retrieve critical semantic information") { }

    }

}
