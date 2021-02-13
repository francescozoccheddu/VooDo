namespace VooDo.Errors
{

    public sealed class CompilerOptionsException : VooDoException
    {

        public string Property { get; }

        internal CompilerOptionsException(string _message, string _property) : base(_message)
        {
            Property = _property;
        }

    }

}
