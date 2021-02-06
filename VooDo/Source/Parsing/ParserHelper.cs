using Antlr4.Runtime;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using System;
using System.Linq;

using VooDo.Parsing.Generated;
using VooDo.Transformation;

namespace VooDo.Parsing
{

    internal static class ParserHelper
    {


        internal static TextSpan GetSpan(this ParserRuleContext _context)
            => new TextSpan(_context.Start.StartIndex, _context.Stop?.StopIndex ?? _context.Start.StopIndex);
        internal static TextSpan GetSpan(this IToken _token)
            => new TextSpan(_token.StartIndex, _token.StopIndex);

        internal static TNode From<TNode>(this TNode _node, ParserRuleContext _context, bool _applyToDescendants = false) where TNode : SyntaxNode
            => _node.WithOriginalSpan(_context.GetSpan(), _applyToDescendants);
        internal static SyntaxToken From(this SyntaxToken _node, ParserRuleContext _context)
            => _node.WithOriginalSpan(_context.GetSpan());
        internal static TNode From<TNode>(this TNode _node, IToken _token, bool _applyToDescendants = false) where TNode : SyntaxNode
            => _node.WithOriginalSpan(_token.GetSpan(), _applyToDescendants);
        internal static SyntaxToken From(this SyntaxToken _node, IToken _token)
            => _node.WithOriginalSpan(_token.GetSpan());

        private static object Literal(string _literal)
            => SyntaxFactory.ParseExpression(_literal).DescendantNodesAndSelf().OfType<LiteralExpressionSyntax>().Single().Token.Value;

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

        internal static SyntaxKind TokenKind(IToken _token)
        {
            switch (_token.Type)
            {
                case VooDoParser.BOOL:
                return SyntaxKind.BoolKeyword;
                case VooDoParser.BYTE:
                return SyntaxKind.ByteKeyword;
                case VooDoParser.CHAR:
                return SyntaxKind.CharKeyword;
                case VooDoParser.DECIMAL:
                return SyntaxKind.DecimalKeyword;
                case VooDoParser.DOUBLE:
                return SyntaxKind.DoubleKeyword;
                case VooDoParser.FLOAT:
                return SyntaxKind.FloatKeyword;
                case VooDoParser.IN:
                return SyntaxKind.InKeyword;
                case VooDoParser.INT:
                return SyntaxKind.IntKeyword;
                case VooDoParser.LONG:
                return SyntaxKind.LongKeyword;
                case VooDoParser.OBJECT:
                return SyntaxKind.ObjectKeyword;
                case VooDoParser.OUT:
                return SyntaxKind.OutKeyword;
                case VooDoParser.REF:
                return SyntaxKind.RefKeyword;
                case VooDoParser.SBYTE:
                return SyntaxKind.SByteKeyword;
                case VooDoParser.SHORT:
                return SyntaxKind.ShortKeyword;
                case VooDoParser.STRING:
                return SyntaxKind.StringKeyword;
                case VooDoParser.UINT:
                return SyntaxKind.UIntKeyword;
                case VooDoParser.ULONG:
                return SyntaxKind.ULongKeyword;
                case VooDoParser.USHORT:
                return SyntaxKind.UShortKeyword;
                case VooDoParser.TRUE:
                return SyntaxKind.ULongKeyword;
                case VooDoParser.FALSE:
                return SyntaxKind.UShortKeyword;
                case VooDoParser.NULL:
                return SyntaxKind.ULongKeyword;
                case VooDoParser.DEFAULT:
                return SyntaxKind.DefaultKeyword;
                case VooDoParser.PLUS:
                return SyntaxKind.PlusToken;
                case VooDoParser.MINUS:
                return SyntaxKind.MinusToken;
                case VooDoParser.NOT:
                return SyntaxKind.ExclamationToken;
                case VooDoParser.BCOMPL:
                return SyntaxKind.TildeToken;
                case VooDoParser.MUL:
                return SyntaxKind.AsteriskToken;
                case VooDoParser.DIV:
                return SyntaxKind.SlashToken;
                case VooDoParser.MOD:
                return SyntaxKind.PercentToken;
                case VooDoParser.AND:
                return SyntaxKind.AmpersandToken;
                case VooDoParser.OR:
                return SyntaxKind.BarToken;
                case VooDoParser.XOR:
                return SyntaxKind.CaretToken;
                case VooDoParser.LT:
                return SyntaxKind.LessThanToken;
                case VooDoParser.GT:
                return SyntaxKind.GreaterThanToken;
                case VooDoParser.LAND:
                return SyntaxKind.AmpersandAmpersandToken;
                case VooDoParser.LOR:
                return SyntaxKind.BarBarToken;
                case VooDoParser.EQ:
                return SyntaxKind.EqualsEqualsToken;
                case VooDoParser.NEQ:
                return SyntaxKind.ExclamationEqualsToken;
                case VooDoParser.LE:
                return SyntaxKind.LessThanEqualsToken;
                case VooDoParser.GE:
                return SyntaxKind.GreaterThanEqualsToken;
                case VooDoParser.LSH:
                return SyntaxKind.LessThanLessThanToken;
                case VooDoParser.RSH:
                return SyntaxKind.GreaterThanGreaterThanToken;
                case VooDoParser.NULLC:
                return SyntaxKind.QuestionQuestionToken;
                case VooDoParser.IS:
                return SyntaxKind.IsKeyword;
                case VooDoParser.ASSIGN:
                return SyntaxKind.EqualsToken;
                case VooDoParser.ASSIGN_ADD:
                return SyntaxKind.PlusEqualsToken;
                case VooDoParser.ASSIGN_SUB:
                return SyntaxKind.MinusEqualsToken;
                case VooDoParser.ASSIGN_MUL:
                return SyntaxKind.AsteriskEqualsToken;
                case VooDoParser.ASSIGN_DIV:
                return SyntaxKind.SlashEqualsToken;
                case VooDoParser.ASSIGN_MOD:
                return SyntaxKind.PercentEqualsToken;
                case VooDoParser.ASSIGN_AND:
                return SyntaxKind.AmpersandEqualsToken;
                case VooDoParser.ASSIGN_OR:
                return SyntaxKind.BarEqualsToken;
                case VooDoParser.ASSIGN_XOR:
                return SyntaxKind.CaretEqualsToken;
                case VooDoParser.ASSIGN_LSH:
                return SyntaxKind.LessThanLessThanEqualsToken;
                case VooDoParser.ASSIGN_RSH:
                return SyntaxKind.GreaterThanGreaterThanEqualsToken;
                case VooDoParser.ASSIGN_NULLC:
                return SyntaxKind.QuestionQuestionEqualsToken;
                case VooDoParser.AS:
                return SyntaxKind.AsKeyword;
                case VooDoParser.STATIC:
                return SyntaxKind.StaticKeyword;
                case VooDoParser.NEW:
                return SyntaxKind.NewKeyword;
                case VooDoParser.USING:
                return SyntaxKind.UsingKeyword;
                case VooDoParser.VAR:
                return SyntaxKind.VarKeyword;
                case VooDoParser.SEMICOLON:
                return SyntaxKind.SemicolonToken;
                case VooDoParser.COLON:
                return SyntaxKind.ColonToken;
                case VooDoParser.DOT:
                return SyntaxKind.DotToken;
                case VooDoParser.DCOLON:
                return SyntaxKind.ColonColonToken;
                case VooDoParser.COMMA:
                return SyntaxKind.CommaToken;
                case VooDoParser.OPEN_BRACE:
                return SyntaxKind.OpenBraceToken;
                case VooDoParser.OPEN_BRACKET:
                return SyntaxKind.OpenBracketToken;
                case VooDoParser.OPEN_PARENS:
                return SyntaxKind.OpenParenToken;
                case VooDoParser.CLOSE_BRACE:
                return SyntaxKind.CloseBraceToken;
                case VooDoParser.CLOSE_BRACKET:
                return SyntaxKind.CloseBracketToken;
                case VooDoParser.CLOSE_PARENS:
                return SyntaxKind.CloseParenToken;
                case VooDoParser.ELSE:
                return SyntaxKind.ElseKeyword;
                case VooDoParser.IF:
                return SyntaxKind.IfKeyword;
                case VooDoParser.INTERR:
                return SyntaxKind.QuestionToken;
                case VooDoParser.RETURN:
                return SyntaxKind.ReturnKeyword;
                default:
                throw new ArgumentException("Unexpected token", nameof(_token));
            }
        }

        internal static SyntaxKind LiteralExpressionKind(IToken _token)
        {
            switch (_token.Type)
            {
                case VooDoParser.TRUE:
                return SyntaxKind.TrueLiteralExpression;
                case VooDoParser.FALSE:
                return SyntaxKind.FalseLiteralExpression;
                case VooDoParser.NULL:
                return SyntaxKind.NullLiteralExpression;
                case VooDoParser.DEFAULT:
                return SyntaxKind.DefaultLiteralExpression;
                default:
                throw new ArgumentException("Unexpected token", nameof(_token));
            }
        }

        internal static SyntaxKind UnaryExpressionKind(IToken _token)
        {
            switch (_token.Type)
            {
                case VooDoParser.PLUS:
                return SyntaxKind.UnaryPlusExpression;
                case VooDoParser.MINUS:
                return SyntaxKind.UnaryMinusExpression;
                case VooDoParser.NOT:
                return SyntaxKind.LogicalNotExpression;
                case VooDoParser.BCOMPL:
                return SyntaxKind.BitwiseNotExpression;
                default:
                throw new ArgumentException("Unexpected token", nameof(_token));
            }
        }

        internal static SyntaxKind BinaryExpressionKind(IToken _token)
        {
            switch (_token.Type)
            {
                case VooDoParser.MUL:
                return SyntaxKind.MultiplyExpression;
                case VooDoParser.DIV:
                return SyntaxKind.DivideExpression;
                case VooDoParser.MOD:
                return SyntaxKind.ModuloExpression;
                case VooDoParser.AND:
                return SyntaxKind.BitwiseAndExpression;
                case VooDoParser.OR:
                return SyntaxKind.BitwiseOrExpression;
                case VooDoParser.XOR:
                return SyntaxKind.ExclusiveOrExpression;
                case VooDoParser.LT:
                return SyntaxKind.LessThanExpression;
                case VooDoParser.GT:
                return SyntaxKind.GreaterThanExpression;
                case VooDoParser.LAND:
                return SyntaxKind.LogicalAndExpression;
                case VooDoParser.LOR:
                return SyntaxKind.LogicalOrExpression;
                case VooDoParser.EQ:
                return SyntaxKind.EqualsExpression;
                case VooDoParser.NEQ:
                return SyntaxKind.NotEqualsExpression;
                case VooDoParser.LE:
                return SyntaxKind.LessThanOrEqualExpression;
                case VooDoParser.GE:
                return SyntaxKind.GreaterThanOrEqualExpression;
                case VooDoParser.LSH:
                return SyntaxKind.LeftShiftExpression;
                case VooDoParser.RSH:
                return SyntaxKind.RightShiftExpression;
                case VooDoParser.NULLC:
                return SyntaxKind.CoalesceExpression;
                case VooDoParser.IS:
                return SyntaxKind.IsExpression;
                case VooDoParser.AS:
                return SyntaxKind.AsExpression;
                case VooDoParser.PLUS:
                return SyntaxKind.AddExpression;
                case VooDoParser.MINUS:
                return SyntaxKind.SubtractExpression;
                default:
                throw new ArgumentException("Unexpected token", nameof(_token));
            }
        }

        internal static SyntaxKind AssignmentExpressionKind(IToken _token)
        {
            switch (_token.Type)
            {
                case VooDoParser.ASSIGN:
                return SyntaxKind.SimpleAssignmentExpression;
                case VooDoParser.ASSIGN_ADD:
                return SyntaxKind.AddAssignmentExpression;
                case VooDoParser.ASSIGN_SUB:
                return SyntaxKind.SubtractAssignmentExpression;
                case VooDoParser.ASSIGN_MUL:
                return SyntaxKind.MultiplyAssignmentExpression;
                case VooDoParser.ASSIGN_DIV:
                return SyntaxKind.DivideAssignmentExpression;
                case VooDoParser.ASSIGN_MOD:
                return SyntaxKind.ModuloAssignmentExpression;
                case VooDoParser.ASSIGN_AND:
                return SyntaxKind.AndAssignmentExpression;
                case VooDoParser.ASSIGN_OR:
                return SyntaxKind.OrAssignmentExpression;
                case VooDoParser.ASSIGN_XOR:
                return SyntaxKind.ExclusiveOrAssignmentExpression;
                case VooDoParser.ASSIGN_LSH:
                return SyntaxKind.LeftShiftAssignmentExpression;
                case VooDoParser.ASSIGN_RSH:
                return SyntaxKind.RightShiftAssignmentExpression;
                case VooDoParser.ASSIGN_NULLC:
                return SyntaxKind.CoalesceAssignmentExpression;
                default:
                throw new ArgumentException("Unexpected token", nameof(_token));
            }
        }

    }

}
