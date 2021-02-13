using Microsoft.CodeAnalysis;

using System.Collections.Immutable;

using VooDo.AST;
using VooDo.Compilation.Emission;

namespace VooDo.Errors.Problems
{

    public sealed class GlobalTypeInferenceProblem : Problem
    {

        private static string GetMessage(bool _hasCandidates, string? _name) => _hasCandidates switch
        {
            false when _name is null => "No candidate type found for global expression",
            true when _name is null => "Multiple candidate type found for global expression",
            false => $"No candidate type found for global '{_name}'",
            true => $"Multiple candidate type found for global '{_name}'"
        };

        public ImmutableArray<ITypeSymbol> CandidateTypes { get; }
        public GlobalPrototype Prototype { get; }

        public GlobalTypeInferenceProblem(ImmutableArray<ITypeSymbol> _candidateTypes, GlobalPrototype _prototype)
            : base(EKind.Semantic, ESeverity.Error, (Node?) _prototype.Expression ?? _prototype.Declaration!, GetMessage(!_candidateTypes.IsEmpty, _prototype.Global.Name?.ToString()))
        {
            CandidateTypes = _candidateTypes;
            Prototype = _prototype;
        }

    }

}
