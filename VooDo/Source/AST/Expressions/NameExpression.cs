using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;

using VooDo.Compilation;
using VooDo.AST.Names;
using VooDo.Compilation;
using VooDo.Runtime;
using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public sealed record NameExpression(bool IsControllerOf, Identifier Name) : NameOrMemberAccessExpression
    {

        #region Overrides

        internal override ExpressionSyntax EmitNode(Scope _scope, Marker _marker)
        {
            bool isGlobal = _scope.IsGlobal(Name);
            if (!isGlobal && IsControllerOf)
            {
                throw new InvalidOperationException();
            }
            ExpressionSyntax result;
            IdentifierNameSyntax name = SyntaxFactory.IdentifierName(Name.EmitToken(_marker));
            result = isGlobal
                ? SyntaxFactoryHelper.MemberAccess(name, IsControllerOf ? nameof(Variable<object>.ControllerFactory) : nameof(Variable<object>.Value))
                : (ExpressionSyntax) name;
            return result.Own(_marker, this);
        }

        public override IEnumerable<Identifier> Children => new[] { Name };
        public override string ToString() => (IsControllerOf ? "$" : "") + $"{Name}";

        #endregion

    }

}
