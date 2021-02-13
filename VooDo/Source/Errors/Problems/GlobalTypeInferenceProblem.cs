using Microsoft.CodeAnalysis;

using System.Collections.Immutable;

using VooDo.AST.Expressions;
using VooDo.AST.Statements;

namespace VooDo.Errors.Problems
{

    public sealed class GlobalTypeInferenceProblem : Problem
    {

        private static string GetMessageForNamedGlobal(bool _hasCandidates, string _name) => _hasCandidates switch
        {
            false => $"No candidate type found for global '{_name}'",
            true => $"Multiple candidate type found for global '{_name}'",
        };

        private static string GetMessageForAnonymousGlobal(bool _hasCandidates) => _hasCandidates switch
        {
            false => "No candidate type found for global expression",
            true => "Multiple candidate type found for global expression",
        };

        public ImmutableArray<ITypeSymbol> CandidateTypes { get; }
        public GlobalExpression? GlobalExpression { get; }
        public DeclarationStatement? Declaration { get; }
        public DeclarationStatement.Declarator? Declarator { get; }
        public bool IsAnonymous => Declarator is null;

        public GlobalTypeInferenceProblem(ImmutableArray<ITypeSymbol> _candidateTypes, DeclarationStatement _declaration, DeclarationStatement.Declarator _declarator)
            : base(EKind.Semantic, ESeverity.Error, _declarator, GetMessageForNamedGlobal(!_candidateTypes.IsEmpty, _declarator.Name))
        {
            CandidateTypes = _candidateTypes;
            Declaration = _declaration;
            Declarator = _declarator;
            GlobalExpression = null;
        }

        public GlobalTypeInferenceProblem(ImmutableArray<ITypeSymbol> _candidateTypes, GlobalExpression _globalExpression)
            : base(EKind.Semantic, ESeverity.Error, _globalExpression, GetMessageForAnonymousGlobal(!_candidateTypes.IsEmpty))
        {
            CandidateTypes = _candidateTypes;
            GlobalExpression = _globalExpression;
            Declaration = null;
            Declarator = null;
        }

    }

}
