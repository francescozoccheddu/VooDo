using System;

using VooDo.Language.AST.Expressions;
using VooDo.Language.AST.Statements;

namespace VooDo.Language
{

    internal static class GrammarConstants
    {

        internal static string Token(this AssignmentStatement.EKind _kind) => _kind switch
        {
            AssignmentStatement.EKind.Simple => "=",
            AssignmentStatement.EKind.Add => "+=",
            AssignmentStatement.EKind.Subtract => "-=",
            AssignmentStatement.EKind.Multiply => "*=",
            AssignmentStatement.EKind.Divide => "/=",
            AssignmentStatement.EKind.Modulo => "%=",
            AssignmentStatement.EKind.LeftShift => "<<=",
            AssignmentStatement.EKind.RightShift => ">>=",
            AssignmentStatement.EKind.BitwiseAnd => "&=",
            AssignmentStatement.EKind.BitwiseOr => "|=",
            AssignmentStatement.EKind.BitwiseXor => "^=",
            AssignmentStatement.EKind.Coalesce => "??=",
            _ => throw new NotImplementedException(),
        };

        internal static string Token(this BinaryExpression.EKind _kind) => _kind switch
        {
            BinaryExpression.EKind.Add => "+",
            BinaryExpression.EKind.Subtract => "-",
            BinaryExpression.EKind.Multiply => "*",
            BinaryExpression.EKind.Divide => "/",
            BinaryExpression.EKind.Modulo => "%",
            BinaryExpression.EKind.LeftShift => "<<",
            BinaryExpression.EKind.RightShift => ">>",
            BinaryExpression.EKind.Equals => "==",
            BinaryExpression.EKind.NotEquals => "!=",
            BinaryExpression.EKind.LessThan => "<",
            BinaryExpression.EKind.LessThanOrEqual => "<=",
            BinaryExpression.EKind.GreaterThan => ">",
            BinaryExpression.EKind.GreaterThanOrEqual => ">=",
            BinaryExpression.EKind.Coalesce => "??",
            BinaryExpression.EKind.LogicAnd => "&&",
            BinaryExpression.EKind.LogicOr => "||",
            BinaryExpression.EKind.BitwiseAnd => "&",
            BinaryExpression.EKind.BitwiseOr => "|",
            BinaryExpression.EKind.BitwiseXor => "^",
            _ => throw new NotImplementedException(),
        };

        internal static string Token(this UnaryExpression.EKind _kind) => _kind switch
        {
            UnaryExpression.EKind.Plus => "+",
            UnaryExpression.EKind.Minus => "-",
            UnaryExpression.EKind.LogicNot => "!",
            UnaryExpression.EKind.BitwiseNot => "~",
            _ => throw new NotImplementedException(),
        };

        internal static string Token(this InvocationExpression.Argument.EKind _kind) => _kind switch
        {
            InvocationExpression.Argument.EKind.Value => "",
            InvocationExpression.Argument.EKind.Ref => "ref",
            InvocationExpression.Argument.EKind.Out => "out",
            _ => throw new NotImplementedException(),
        };

        internal const string globalKeyword = "global";
        internal const string globKeyword = "glob";
        internal const string initKeyword = "init";
        internal const string statementEndToken = ";";
        internal const string ifKeyword = "if";
        internal const string elseKeyword = "else";
        internal const string returnKeyword = "return";
        internal const string nameEqualsToken = "=";
        internal const string usingKeyword = "using";
        internal const string staticKeyword = "static";
        internal const string isKeyword = "is";
        internal const string asKeyword = "as";
        internal const string newKeyword = "new";
        internal const string defaultKeyword = "default";

    }

}
