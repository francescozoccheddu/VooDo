using Microsoft.CodeAnalysis;

using System.Collections.Immutable;

using VooDo.Language.AST;
using VooDo.Language.AST.Names;
using VooDo.Language.Linking;

namespace VooDo.Source.Linking
{
    public class Test
    {

        public static string Emit(Script _script)
        {
            return _script.EmitNode(new Scope(), new Marker(), new Identifier[] { "Test" }.ToImmutableArray(), ComplexType.Parse("float"))
                .NormalizeWhitespace()
                .ToFullString();
        }

    }
}
