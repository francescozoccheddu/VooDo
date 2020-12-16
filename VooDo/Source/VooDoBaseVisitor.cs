//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.9
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from C:\Users\franc\Documents\Visual Studio 2019\Projects\VooDo\VooDo\VooDo\Parsing\ANTLR\\VooDo.g4 by ANTLR 4.9

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace VooDo.Parsing.ANTLR.Generated {
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;
using ParserRuleContext = Antlr4.Runtime.ParserRuleContext;

/// <summary>
/// This class provides an empty implementation of <see cref="IVooDoVisitor{Result}"/>,
/// which can be extended to create a visitor which only needs to handle a subset
/// of the available methods.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.9")]
[System.Diagnostics.DebuggerNonUserCode]
[System.CLSCompliant(false)]
public partial class VooDoBaseVisitor<Result> : AbstractParseTreeVisitor<Result>, IVooDoVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by the <c>MulOpExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitMulOpExpr([NotNull] VooDoParser.MulOpExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>BoolLitExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitBoolLitExpr([NotNull] VooDoParser.BoolLitExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>LogNotExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitLogNotExpr([NotNull] VooDoParser.LogNotExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>GtOpExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitGtOpExpr([NotNull] VooDoParser.GtOpExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>DivOpExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitDivOpExpr([NotNull] VooDoParser.DivOpExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>PosOpExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitPosOpExpr([NotNull] VooDoParser.PosOpExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>MemOpExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitMemOpExpr([NotNull] VooDoParser.MemOpExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>StringLitExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitStringLitExpr([NotNull] VooDoParser.StringLitExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>NameExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitNameExpr([NotNull] VooDoParser.NameExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>EqOpExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitEqOpExpr([NotNull] VooDoParser.EqOpExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>IndexExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitIndexExpr([NotNull] VooDoParser.IndexExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>BinIntLitExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitBinIntLitExpr([NotNull] VooDoParser.BinIntLitExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>BwLstOpExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitBwLstOpExpr([NotNull] VooDoParser.BwLstOpExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>BwOrOpExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitBwOrOpExpr([NotNull] VooDoParser.BwOrOpExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>HexIntLitExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitHexIntLitExpr([NotNull] VooDoParser.HexIntLitExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>CastExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitCastExpr([NotNull] VooDoParser.CastExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>NullableMemOpExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitNullableMemOpExpr([NotNull] VooDoParser.NullableMemOpExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>BwXorOpExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitBwXorOpExpr([NotNull] VooDoParser.BwXorOpExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>LogOrOpExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitLogOrOpExpr([NotNull] VooDoParser.LogOrOpExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>SubOpExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitSubOpExpr([NotNull] VooDoParser.SubOpExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>NullCoalOpExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitNullCoalOpExpr([NotNull] VooDoParser.NullCoalOpExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>NullLitExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitNullLitExpr([NotNull] VooDoParser.NullLitExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>IfElseOpExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitIfElseOpExpr([NotNull] VooDoParser.IfElseOpExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>LeOpExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitLeOpExpr([NotNull] VooDoParser.LeOpExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>NegOpExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitNegOpExpr([NotNull] VooDoParser.NegOpExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>BwNegExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitBwNegExpr([NotNull] VooDoParser.BwNegExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>BwRstOpExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitBwRstOpExpr([NotNull] VooDoParser.BwRstOpExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>GroupExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitGroupExpr([NotNull] VooDoParser.GroupExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>OctIntLitExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitOctIntLitExpr([NotNull] VooDoParser.OctIntLitExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>CallOpExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitCallOpExpr([NotNull] VooDoParser.CallOpExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>LogAndOpExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitLogAndOpExpr([NotNull] VooDoParser.LogAndOpExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>GeOpExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitGeOpExpr([NotNull] VooDoParser.GeOpExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>LtOpExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitLtOpExpr([NotNull] VooDoParser.LtOpExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>ModOpExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitModOpExpr([NotNull] VooDoParser.ModOpExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>SumOpExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitSumOpExpr([NotNull] VooDoParser.SumOpExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>NeqOpExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitNeqOpExpr([NotNull] VooDoParser.NeqOpExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>RealLitExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitRealLitExpr([NotNull] VooDoParser.RealLitExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>BwAndOpExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitBwAndOpExpr([NotNull] VooDoParser.BwAndOpExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>DecIntLitExpr</c>
	/// labeled alternative in <see cref="VooDoParser.expr"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitDecIntLitExpr([NotNull] VooDoParser.DecIntLitExprContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>SequenceStat</c>
	/// labeled alternative in <see cref="VooDoParser.stat"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitSequenceStat([NotNull] VooDoParser.SequenceStatContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>IfElseStat</c>
	/// labeled alternative in <see cref="VooDoParser.stat"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitIfElseStat([NotNull] VooDoParser.IfElseStatContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>WhileStat</c>
	/// labeled alternative in <see cref="VooDoParser.stat"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitWhileStat([NotNull] VooDoParser.WhileStatContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>ForeachStat</c>
	/// labeled alternative in <see cref="VooDoParser.stat"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitForeachStat([NotNull] VooDoParser.ForeachStatContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>AssignmentStat</c>
	/// labeled alternative in <see cref="VooDoParser.stat"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitAssignmentStat([NotNull] VooDoParser.AssignmentStatContext context) { return VisitChildren(context); }
	/// <summary>
	/// Visit a parse tree produced by the <c>LinkStat</c>
	/// labeled alternative in <see cref="VooDoParser.stat"/>.
	/// <para>
	/// The default implementation returns the result of calling <see cref="AbstractParseTreeVisitor{Result}.VisitChildren(IRuleNode)"/>
	/// on <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	public virtual Result VisitLinkStat([NotNull] VooDoParser.LinkStatContext context) { return VisitChildren(context); }
}
} // namespace VooDo.Parsing.ANTLR.Generated
