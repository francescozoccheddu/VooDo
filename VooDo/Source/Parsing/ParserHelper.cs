#nullable enable

using Antlr4.Runtime;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Linq;

using VooDo.Factory;
using VooDo.Parsing.Generated;
using VooDo.Transformation;

namespace VooDo.Parsing
{

    internal static class ParserHelper
    {


        internal static Origin GetSpan(this ParserRuleContext _context)
            => Origin.FromSource(_context.Start.StartIndex, _context.Stop?.StopIndex ?? _context.Start.StopIndex);
        internal static Origin GetSpan(this IToken _token)
            => Origin.FromSource(_token.StartIndex, _token.StopIndex);

        internal static TNode From<TNode>(this TNode _node, ParserRuleContext _context, bool _applyToDescendants = false) where TNode : SyntaxNode
            => _node.WithOrigin(_context.GetSpan(), _applyToDescendants);
        internal static SyntaxToken From(this SyntaxToken _node, ParserRuleContext _context)
            => _node.WithOrigin(_context.GetSpan());
        internal static TNode From<TNode>(this TNode _node, IToken _token, bool _applyToDescendants = false) where TNode : SyntaxNode
            => _node.WithOrigin(_token.GetSpan(), _applyToDescendants);
        internal static SyntaxToken From(this SyntaxToken _node, IToken _token)
            => _node.WithOrigin(_token.GetSpan());

        private static object Literal(string _literal)
            => SyntaxFactory.ParseExpression(_literal).DescendantNodesAndSelf().OfType<LiteralExpressionSyntax>().Single().Token.Value!;

        internal static string StringLiteral(string _literal)
            => (string) Literal(_literal);

        internal static double RealLiteral(string _literal)
            => (double) Literal(_literal);

        internal static decimal DecimalOrOctalIntegerLiteral(string _literal)
            => (decimal) Literal(_literal);

        internal static decimal BinaryIntegerLiteral(string _literal)
            => (decimal) Literal(_literal);

        internal static decimal HexadecimalIntegerLiteral(string _literal)
            => (decimal) Literal(_literal);

        internal static char CharLiteral(string _literal)
            => (char) Literal(_literal);

        internal static SyntaxKind TokenKind(IToken _token) => _token.Type switch
        {
            VooDoParser.BOOL => SyntaxKind.BoolKeyword,
            VooDoParser.BYTE => SyntaxKind.ByteKeyword,
            VooDoParser.CHAR => SyntaxKind.CharKeyword,
            VooDoParser.DECIMAL => SyntaxKind.DecimalKeyword,
            VooDoParser.DOUBLE => SyntaxKind.DoubleKeyword,
            VooDoParser.FLOAT => SyntaxKind.FloatKeyword,
            VooDoParser.IN => SyntaxKind.InKeyword,
            VooDoParser.INT => SyntaxKind.IntKeyword,
            VooDoParser.LONG => SyntaxKind.LongKeyword,
            VooDoParser.OBJECT => SyntaxKind.ObjectKeyword,
            VooDoParser.OUT => SyntaxKind.OutKeyword,
            VooDoParser.REF => SyntaxKind.RefKeyword,
            VooDoParser.SBYTE => SyntaxKind.SByteKeyword,
            VooDoParser.SHORT => SyntaxKind.ShortKeyword,
            VooDoParser.STRING => SyntaxKind.StringKeyword,
            VooDoParser.UINT => SyntaxKind.UIntKeyword,
            VooDoParser.ULONG => SyntaxKind.ULongKeyword,
            VooDoParser.USHORT => SyntaxKind.UShortKeyword,
            VooDoParser.TRUE => SyntaxKind.ULongKeyword,
            VooDoParser.FALSE => SyntaxKind.UShortKeyword,
            VooDoParser.NULL => SyntaxKind.ULongKeyword,
            VooDoParser.DEFAULT => SyntaxKind.DefaultKeyword,
            VooDoParser.PLUS => SyntaxKind.PlusToken,
            VooDoParser.MINUS => SyntaxKind.MinusToken,
            VooDoParser.NOT => SyntaxKind.ExclamationToken,
            VooDoParser.BCOMPL => SyntaxKind.TildeToken,
            VooDoParser.MUL => SyntaxKind.AsteriskToken,
            VooDoParser.DIV => SyntaxKind.SlashToken,
            VooDoParser.MOD => SyntaxKind.PercentToken,
            VooDoParser.AND => SyntaxKind.AmpersandToken,
            VooDoParser.OR => SyntaxKind.BarToken,
            VooDoParser.XOR => SyntaxKind.CaretToken,
            VooDoParser.LT => SyntaxKind.LessThanToken,
            VooDoParser.GT => SyntaxKind.GreaterThanToken,
            VooDoParser.LAND => SyntaxKind.AmpersandAmpersandToken,
            VooDoParser.LOR => SyntaxKind.BarBarToken,
            VooDoParser.EQ => SyntaxKind.EqualsEqualsToken,
            VooDoParser.NEQ => SyntaxKind.ExclamationEqualsToken,
            VooDoParser.LE => SyntaxKind.LessThanEqualsToken,
            VooDoParser.GE => SyntaxKind.GreaterThanEqualsToken,
            VooDoParser.LSH => SyntaxKind.LessThanLessThanToken,
            VooDoParser.RSH => SyntaxKind.GreaterThanGreaterThanToken,
            VooDoParser.NULLC => SyntaxKind.QuestionQuestionToken,
            VooDoParser.IS => SyntaxKind.IsKeyword,
            VooDoParser.ASSIGN => SyntaxKind.EqualsToken,
            VooDoParser.ASSIGN_ADD => SyntaxKind.PlusEqualsToken,
            VooDoParser.ASSIGN_SUB => SyntaxKind.MinusEqualsToken,
            VooDoParser.ASSIGN_MUL => SyntaxKind.AsteriskEqualsToken,
            VooDoParser.ASSIGN_DIV => SyntaxKind.SlashEqualsToken,
            VooDoParser.ASSIGN_MOD => SyntaxKind.PercentEqualsToken,
            VooDoParser.ASSIGN_AND => SyntaxKind.AmpersandEqualsToken,
            VooDoParser.ASSIGN_OR => SyntaxKind.BarEqualsToken,
            VooDoParser.ASSIGN_XOR => SyntaxKind.CaretEqualsToken,
            VooDoParser.ASSIGN_LSH => SyntaxKind.LessThanLessThanEqualsToken,
            VooDoParser.ASSIGN_RSH => SyntaxKind.GreaterThanGreaterThanEqualsToken,
            VooDoParser.ASSIGN_NULLC => SyntaxKind.QuestionQuestionEqualsToken,
            VooDoParser.AS => SyntaxKind.AsKeyword,
            VooDoParser.STATIC => SyntaxKind.StaticKeyword,
            VooDoParser.NEW => SyntaxKind.NewKeyword,
            VooDoParser.USING => SyntaxKind.UsingKeyword,
            VooDoParser.VAR => SyntaxKind.VarKeyword,
            VooDoParser.SEMICOLON => SyntaxKind.SemicolonToken,
            VooDoParser.COLON => SyntaxKind.ColonToken,
            VooDoParser.DOT => SyntaxKind.DotToken,
            VooDoParser.DCOLON => SyntaxKind.ColonColonToken,
            VooDoParser.COMMA => SyntaxKind.CommaToken,
            VooDoParser.OPEN_BRACE => SyntaxKind.OpenBraceToken,
            VooDoParser.OPEN_BRACKET => SyntaxKind.OpenBracketToken,
            VooDoParser.OPEN_PARENS => SyntaxKind.OpenParenToken,
            VooDoParser.CLOSE_BRACE => SyntaxKind.CloseBraceToken,
            VooDoParser.CLOSE_BRACKET => SyntaxKind.CloseBracketToken,
            VooDoParser.CLOSE_PARENS => SyntaxKind.CloseParenToken,
            VooDoParser.ELSE => SyntaxKind.ElseKeyword,
            VooDoParser.IF => SyntaxKind.IfKeyword,
            VooDoParser.INTERR => SyntaxKind.QuestionToken,
            VooDoParser.RETURN => SyntaxKind.ReturnKeyword,
            _ => throw new ArgumentException("Unexpected token", nameof(_token)),
        };

        internal static SyntaxKind LiteralExpressionKind(IToken _token) => _token.Type switch
        {
            VooDoParser.TRUE => SyntaxKind.TrueLiteralExpression,
            VooDoParser.FALSE => SyntaxKind.FalseLiteralExpression,
            VooDoParser.NULL => SyntaxKind.NullLiteralExpression,
            VooDoParser.DEFAULT => SyntaxKind.DefaultLiteralExpression,
            _ => throw new ArgumentException("Unexpected token", nameof(_token)),
        };

        internal static SyntaxKind UnaryExpressionKind(IToken _token) => _token.Type switch
        {
            VooDoParser.PLUS => SyntaxKind.UnaryPlusExpression,
            VooDoParser.MINUS => SyntaxKind.UnaryMinusExpression,
            VooDoParser.NOT => SyntaxKind.LogicalNotExpression,
            VooDoParser.BCOMPL => SyntaxKind.BitwiseNotExpression,
            _ => throw new ArgumentException("Unexpected token", nameof(_token)),
        };

        internal static SyntaxKind BinaryExpressionKind(IToken _token) => _token.Type switch
        {
            VooDoParser.MUL => SyntaxKind.MultiplyExpression,
            VooDoParser.DIV => SyntaxKind.DivideExpression,
            VooDoParser.MOD => SyntaxKind.ModuloExpression,
            VooDoParser.AND => SyntaxKind.BitwiseAndExpression,
            VooDoParser.OR => SyntaxKind.BitwiseOrExpression,
            VooDoParser.XOR => SyntaxKind.ExclusiveOrExpression,
            VooDoParser.LT => SyntaxKind.LessThanExpression,
            VooDoParser.GT => SyntaxKind.GreaterThanExpression,
            VooDoParser.LAND => SyntaxKind.LogicalAndExpression,
            VooDoParser.LOR => SyntaxKind.LogicalOrExpression,
            VooDoParser.EQ => SyntaxKind.EqualsExpression,
            VooDoParser.NEQ => SyntaxKind.NotEqualsExpression,
            VooDoParser.LE => SyntaxKind.LessThanOrEqualExpression,
            VooDoParser.GE => SyntaxKind.GreaterThanOrEqualExpression,
            VooDoParser.LSH => SyntaxKind.LeftShiftExpression,
            VooDoParser.RSH => SyntaxKind.RightShiftExpression,
            VooDoParser.NULLC => SyntaxKind.CoalesceExpression,
            VooDoParser.IS => SyntaxKind.IsExpression,
            VooDoParser.AS => SyntaxKind.AsExpression,
            VooDoParser.PLUS => SyntaxKind.AddExpression,
            VooDoParser.MINUS => SyntaxKind.SubtractExpression,
            _ => throw new ArgumentException("Unexpected token", nameof(_token)),
        };

        internal static SyntaxKind AssignmentExpressionKind(IToken _token) => _token.Type switch
        {
            VooDoParser.ASSIGN => SyntaxKind.SimpleAssignmentExpression,
            VooDoParser.ASSIGN_ADD => SyntaxKind.AddAssignmentExpression,
            VooDoParser.ASSIGN_SUB => SyntaxKind.SubtractAssignmentExpression,
            VooDoParser.ASSIGN_MUL => SyntaxKind.MultiplyAssignmentExpression,
            VooDoParser.ASSIGN_DIV => SyntaxKind.DivideAssignmentExpression,
            VooDoParser.ASSIGN_MOD => SyntaxKind.ModuloAssignmentExpression,
            VooDoParser.ASSIGN_AND => SyntaxKind.AndAssignmentExpression,
            VooDoParser.ASSIGN_OR => SyntaxKind.OrAssignmentExpression,
            VooDoParser.ASSIGN_XOR => SyntaxKind.ExclusiveOrAssignmentExpression,
            VooDoParser.ASSIGN_LSH => SyntaxKind.LeftShiftAssignmentExpression,
            VooDoParser.ASSIGN_RSH => SyntaxKind.RightShiftAssignmentExpression,
            VooDoParser.ASSIGN_NULLC => SyntaxKind.CoalesceAssignmentExpression,
            _ => throw new ArgumentException("Unexpected token", nameof(_token)),
        };

    }

}
