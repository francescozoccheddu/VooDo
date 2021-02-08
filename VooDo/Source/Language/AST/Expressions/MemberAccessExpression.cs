using VooDo.Language.AST.Names;

namespace VooDo.Language.AST.Expressions
{

    public sealed record MemberAccessExpression(ComplexTypeOrExpression Source, bool Coalesce, Identifier Member) : AssignableExpression
    {

        #region Overrides

        public override string ToString() => $"{Source}" + (Coalesce ? "?." : ".") + $"{Member}";

        #endregion

    }

}
