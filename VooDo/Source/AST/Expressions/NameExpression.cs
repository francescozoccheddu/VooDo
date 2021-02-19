using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;

using VooDo.AST.Names;
using VooDo.AST.Statements;
using VooDo.Compiling.Emission;
using VooDo.Problems;
using VooDo.Runtime;
using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public sealed record NameExpression(bool IsControllerOf, Identifier Name) : NameOrMemberAccessExpression
    {

        #region Overrides

        protected override EPrecedence m_Precedence => EPrecedence.Primary;

        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
        {
            Identifier newName = (Identifier) _map(Name).NonNull();
            if (ReferenceEquals(newName, Name))
            {
                return this;
            }
            else
            {
                return this with
                {
                    Name = newName
                };
            }
        }

        internal override SyntaxNode EmitNode(Scope _scope, Tagger _tagger)
        {
            bool isGlobal = _scope.IsGlobal(Name);
            bool isConstant = _scope.IsConstant(Name);
            if (IsControllerOf)
            {
                if (!isGlobal)
                {
                    throw new ControllerOfNonGlobalProblem(this).AsThrowable();
                }
                if (isConstant)
                {
                    throw new ControllerOfConstantProblem(this).AsThrowable();
                }
            }
            if (isConstant && Parent is AssignmentStatement assignment && assignment.Target == this)
            {
                throw new AssignmentOfConstantProblem(this).AsThrowable();
            }
            ExpressionSyntax result;
            IdentifierNameSyntax name = SyntaxFactory.IdentifierName(Name.EmitToken(_tagger));
            result = isGlobal
                ? SyntaxFactoryUtils.MemberAccess(name, IsControllerOf ? nameof(Variable<object>.ControllerFactory) : nameof(Variable<object>.Value))
                : (ExpressionSyntax) name;
            return result.Own(_tagger, this);
        }

        public override IEnumerable<Node> Children => new Node[] { Name };
        public override string ToString() => (IsControllerOf ? "$" : "") + $"{Name}";

        #endregion

    }

}
