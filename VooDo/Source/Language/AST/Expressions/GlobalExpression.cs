using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Linq;

using VooDo.Language.AST.Names;
using VooDo.Language.Linking;
using VooDo.Utils;

namespace VooDo.Language.AST.Expressions
{

    public sealed record GlobalExpression(Expression Controller, Expression? Initializer) : Expression
    {

        #region Members

        private static bool IsValidInitializer(Expression _expression)
            => !_expression.DescendantNodes().Any(_e => _e is GlobalExpression);

        private Expression? m_initializer = Initializer?.Assert(IsValidInitializer);
        public Expression? Initializer
        {
            get => m_initializer;
            init => m_initializer = value?.Assert(IsValidInitializer);
        }

        public bool HasInitializer => Initializer is not null;

        #endregion

        #region Overrides

        internal override InvocationExpressionSyntax EmitNode(Scope _scope, Marker _marker)
        {
            Scope.GlobalDefinition globalDefinition = _scope.AddGlobal(new Global(ComplexTypeOrVar.Var, null, Initializer));
            return SyntaxFactory.InvocationExpression(
                SyntaxFactoryHelper.ThisMemberAccess("SetControllerAndGetValue"),
                SyntaxFactoryHelper.Arguments(
                        new ArgumentSyntax[]
                        {
                            SyntaxFactory.Argument(
                                SyntaxFactoryHelper.ThisMemberAccess(
                                    globalDefinition.Identifier)),
                            SyntaxFactory.Argument(
                                Controller.EmitNode(_scope, _marker))
                            .Own(_marker, Controller)
                        }))
                .Own(_marker, this);
        }
        public override IEnumerable<Expression> Children => HasInitializer ? new Expression[] { Controller, Initializer! } : new Expression[] { Controller };
        public override string ToString() => $"{GrammarConstants.globKeyword} {Controller}" + (HasInitializer ? $" {GrammarConstants.initKeyword} {Initializer}" : "");

        #endregion


    }

}
