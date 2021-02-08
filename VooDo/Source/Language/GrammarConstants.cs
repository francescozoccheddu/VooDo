using System;

using VooDo.Language.AST.Expressions;
using VooDo.Language.AST.Statements;

namespace VooDo.Language
{

    public static class GrammarConstants
    {

        public static string Token(this AssignmentStatement.EKind _kind) => _kind switch
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

        public static string Token(this BinaryExpression.EKind _kind) => _kind switch
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

        public static string Token(this UnaryExpression.EKind _kind) => _kind switch
        {
            UnaryExpression.EKind.Plus => "+",
            UnaryExpression.EKind.Minus => "-",
            UnaryExpression.EKind.LogicNot => "!",
            UnaryExpression.EKind.BitwiseNot => "~",
            _ => throw new NotImplementedException(),
        };

        public static string Token(this InvocationExpression.Argument.EKind _kind) => _kind switch
        {
            InvocationExpression.Argument.EKind.Value => "",
            InvocationExpression.Argument.EKind.Ref => "ref",
            InvocationExpression.Argument.EKind.Out => "out",
            _ => throw new NotImplementedException(),
        };

        public const string globalKeyword = "global";
        public const string globKeyword = "glob";
        public const string initKeyword = "init";
        public const string statementEndToken = ";";
        public const string ifKeyword = "if";
        public const string elseKeyword = "else";
        public const string returnKeyword = "return";
        public const string nameEqualsToken = "=";
        public const string usingKeyword = "using";
        public const string staticKeyword = "static";
        public const string isKeyword = "is";
        public const string asKeyword = "as";
        public const string newKeyword = "new";
        public const string defaultKeyword = "default";

    }

}
