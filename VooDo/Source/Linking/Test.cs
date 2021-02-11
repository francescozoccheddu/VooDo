using Microsoft.CodeAnalysis;

using VooDo.Language.AST;
using VooDo.Language.Linking;

namespace VooDo.Source.Linking
{
    public class Test
    {

        public static string Emit(Script _script)
        {
            return _script.EmitNode(new Scope(), new Marker())
                .NormalizeWhitespace()
                .ToFullString();
        }

    }
}
