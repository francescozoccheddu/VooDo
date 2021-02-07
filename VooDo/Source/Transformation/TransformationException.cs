using Microsoft.CodeAnalysis;

using System;

namespace VooDo.Transformation
{

    public sealed class TransformationException : Exception
    {

        public Diagnostic Diagnostic { get; }

        public TransformationException(Diagnostic _diagnostic)
        {
            if (_diagnostic is null)
            {
                throw new ArgumentNullException(nameof(_diagnostic));
            }
            Diagnostic = _diagnostic;
        }

        public override string Message => Diagnostic.GetMessage();

    }

}
