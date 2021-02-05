using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;

namespace VooDo.Factory
{

    public sealed class ScriptSource
    {

        public static ScriptSource Create(string _source)
        {
            throw new NotImplementedException();
        }

        public static ScriptSource CreateFromCSharp(string _source)
        {
            throw new NotImplementedException();
        }

        public static ScriptSource CreateFromCSharp(CompilationUnitSyntax _source)
        {
            throw new NotImplementedException();
        }

        public void Compile()
        {
        }


    }

}
