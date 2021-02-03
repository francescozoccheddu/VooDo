using Antlr4.Runtime;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using System;

using VooDo.Parsing.Generated;
using VooDo.Transformation;

namespace VooDo.Parsing
{

    internal static class ParserHelper
    {


        internal static TextSpan GetSpan(this ParserRuleContext _context)
            => new TextSpan(_context.Start.StartIndex, _context.Stop.StopIndex);
        internal static TextSpan GetSpan(this IToken _token)
            => new TextSpan(_token.StartIndex, _token.StopIndex);

        internal static TNode Mark<TNode>(this TNode _node, ParserRuleContext _context) where TNode : SyntaxNode
            => _node.WithOriginalSpan(_context.GetSpan());
        internal static SyntaxToken Mark(this SyntaxToken _node, ParserRuleContext _context)
            => _node.WithOriginalSpan(_context.GetSpan());
        internal static TNode Mark<TNode>(this TNode _node, IToken _token, bool _recursive = false) where TNode : SyntaxNode
            => _node.WithOriginalSpan(_token.GetSpan(), _recursive);
        internal static SyntaxToken Mark(this SyntaxToken _node, IToken _token, bool _recursive = false)
            => _node.WithOriginalSpan(_token.GetSpan(), _recursive);

        internal static string StringLiteral(string _literal)
            => throw new NotImplementedException();

        internal static double RealLiteral(string _literal)
            => throw new NotImplementedException();

        internal static decimal DecimalOrOctalIntegerLiteral(string _literal)
            => throw new NotImplementedException();

        internal static decimal BinaryIntegerLiteral(string _literal)
            => throw new NotImplementedException();

        internal static decimal HexadecimalIntegerLiteral(string _literal)
            => throw new NotImplementedException();

        internal static char CharLiteral(string _literal)
            => throw new NotImplementedException();

        internal static SyntaxKind KeywordKind(IToken _token)
        {
            switch (_token.TokenIndex)
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
                default:
                throw new ArgumentException("Unexpected token", nameof(_token));
            }
        }

        internal static SyntaxKind UnaryExpressionKind(IToken _token)
        {
            switch (_token.TokenIndex)
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
            switch (_token.TokenIndex)
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
                default:
                throw new ArgumentException("Unexpected token", nameof(_token));
            }
        }

        internal static SyntaxKind AssignmentExpressionKind(IToken _token)
        {
            switch (_token.TokenIndex)
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
