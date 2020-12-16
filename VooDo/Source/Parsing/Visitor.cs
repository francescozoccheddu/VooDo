using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

using System;
using System.Collections.Generic;
using System.Linq;

using VooDo.AST;
using VooDo.AST.Expressions;
using VooDo.AST.Expressions.Fundamentals;
using VooDo.AST.Expressions.Literals;
using VooDo.AST.Expressions.Operators;
using VooDo.AST.Expressions.Operators.Comparisons;
using VooDo.AST.Expressions.Operators.Operations;
using VooDo.AST.Statements;
using VooDo.Parsing.Generated;
using VooDo.Utils;

namespace VooDo.Parsing
{

    internal sealed class Visitor : VooDoBaseVisitor<ASTBase>
    {

        private T Get<T>(ParserRuleContext _context) where T : ASTBase => (T) Visit(_context);
        private Expr Get(VooDoParser.ExprContext _context) => Get<Expr>(_context);
        private Expr[] Get(IEnumerable<VooDoParser.ExprContext> _context) => _context.Select(_c => Get<Expr>(_c)).ToArray();
        private Stat Get(VooDoParser.StatContext _context) => Get<Stat>(_context);
        private Stat[] Get(IEnumerable<VooDoParser.StatContext> _context) => _context.Select(_c => Get<Stat>(_c)).ToArray();

        public override ASTBase VisitAssignmentStat([NotNull] VooDoParser.AssignmentStatContext _context) => new AssignmentStat(Get(_context.tgtExpr), Get(_context.srcExpr));
        public override ASTBase VisitBinIntLitExpr([NotNull] VooDoParser.BinIntLitExprContext _context) => new IntLitExpr(Convert.ToInt32(_context.value.Text.Substring(2), 2));
        public override ASTBase VisitBoolLitExpr([NotNull] VooDoParser.BoolLitExprContext _context) => new BoolLitExpr(_context.value.Text == "true");
        public override ASTBase VisitBwAndOpExpr([NotNull] VooDoParser.BwAndOpExprContext _context) => new BwAndOpExpr(Get(_context.lExpr), Get(_context.rExpr));
        public override ASTBase VisitBwLstOpExpr([NotNull] VooDoParser.BwLstOpExprContext _context) => new BwAndOpExpr(Get(_context.lExpr), Get(_context.rExpr));
        public override ASTBase VisitBwNegExpr([NotNull] VooDoParser.BwNegExprContext _context) => new BwLstOpExpr(Get(_context.srcExpr));
        public override ASTBase VisitBwOrOpExpr([NotNull] VooDoParser.BwOrOpExprContext _context) => new BwOrOpExpr(Get(_context.lExpr), Get(_context.rExpr));
        public override ASTBase VisitBwRstOpExpr([NotNull] VooDoParser.BwRstOpExprContext _context) => new BwRstOpExpr(Get(_context.lExpr), Get(_context.rExpr));
        public override ASTBase VisitBwXorOpExpr([NotNull] VooDoParser.BwXorOpExprContext _context) => new BwXorOpExpr(Get(_context.lExpr), Get(_context.rExpr));
        public override ASTBase VisitCallOpExpr([NotNull] VooDoParser.CallOpExprContext _context) => new CallExpr(Get(_context.srcExpr), Get(_context._argsExpr));
        public override ASTBase VisitCastExpr([NotNull] VooDoParser.CastExprContext _context) => new CastExpr(Get(_context.srcExpr), Get(_context.typeExpr));
        public override ASTBase VisitDecIntLitExpr([NotNull] VooDoParser.DecIntLitExprContext _context) => new IntLitExpr(Convert.ToInt32(_context.value.Text, 10));
        public override ASTBase VisitDivOpExpr([NotNull] VooDoParser.DivOpExprContext _context) => new DivOpExpr(Get(_context.lExpr), Get(_context.rExpr));
        public override ASTBase VisitEqOpExpr([NotNull] VooDoParser.EqOpExprContext _context) => new EqOpExpr(Get(_context.lExpr), Get(_context.rExpr));
        public override ASTBase VisitForeachStat([NotNull] VooDoParser.ForeachStatContext _context) => new ForeachStat(Get(_context.tgtExpr), Get(_context.srcExpr), Get(_context.doStat));
        public override ASTBase VisitGeOpExpr([NotNull] VooDoParser.GeOpExprContext _context) => new GeOpExpr(Get(_context.lExpr), Get(_context.rExpr));
        public override ASTBase VisitGroupExpr([NotNull] VooDoParser.GroupExprContext _context) => Get(_context.srcExpr);
        public override ASTBase VisitGtOpExpr([NotNull] VooDoParser.GtOpExprContext _context) => new GtOpExpr(Get(_context.lExpr), Get(_context.rExpr));
        public override ASTBase VisitHexIntLitExpr([NotNull] VooDoParser.HexIntLitExprContext _context) => new IntLitExpr(Convert.ToInt32(_context.value.Text.Substring(2), 16));
        public override ASTBase VisitIfElseOpExpr([NotNull] VooDoParser.IfElseOpExprContext _context) => new IfElseExpr(Get(_context.condExpr), Get(_context.thenExpr), Get(_context.elseExpr));
        public override ASTBase VisitIfElseStat([NotNull] VooDoParser.IfElseStatContext _context)
            => new IfElseStat(Get(_context.condExpr), Get(_context.thenStat), _context.elseStat != null ? Get(_context.elseStat) : null);
        public override ASTBase VisitIndexExpr([NotNull] VooDoParser.IndexExprContext _context) => new IndexExpr(Get(_context.srcExpr), Get(_context._argsExpr), false);
        public override ASTBase VisitLeOpExpr([NotNull] VooDoParser.LeOpExprContext _context) => new LeOpExpr(Get(_context.lExpr), Get(_context.rExpr));
        public override ASTBase VisitLinkStat([NotNull] VooDoParser.LinkStatContext _context) => new LinkStat(Get(_context.tgtExpr), Get(_context.srcExpr));
        public override ASTBase VisitLogAndOpExpr([NotNull] VooDoParser.LogAndOpExprContext _context) => new LogAndOpExpr(Get(_context.lExpr), Get(_context.rExpr));
        public override ASTBase VisitLogNotExpr([NotNull] VooDoParser.LogNotExprContext _context) => new LogNotOpExpr(Get(_context.srcExpr));
        public override ASTBase VisitLogOrOpExpr([NotNull] VooDoParser.LogOrOpExprContext _context) => new LogOrOpExpr(Get(_context.lExpr), Get(_context.rExpr));
        public override ASTBase VisitLtOpExpr([NotNull] VooDoParser.LtOpExprContext _context) => new LtOpExpr(Get(_context.lExpr), Get(_context.rExpr));
        public override ASTBase VisitMemOpExpr([NotNull] VooDoParser.MemOpExprContext _context) => new MemberOpExpr(Get(_context.srcExpr), Get(_context.memberExpr), false);
        public override ASTBase VisitModOpExpr([NotNull] VooDoParser.ModOpExprContext _context) => new ModOpExpr(Get(_context.lExpr), Get(_context.rExpr));
        public override ASTBase VisitMulOpExpr([NotNull] VooDoParser.MulOpExprContext _context) => new MulOpExpr(Get(_context.lExpr), Get(_context.rExpr));
        public override ASTBase VisitNameExpr([NotNull] VooDoParser.NameExprContext _context) => new VarExpr(new QualifiedName(_context._path.Select(_p => new Name(_p.Text))));
        public override ASTBase VisitNegOpExpr([NotNull] VooDoParser.NegOpExprContext _context) => new NegOpExpr(Get(_context.srcExpr));
        public override ASTBase VisitNeqOpExpr([NotNull] VooDoParser.NeqOpExprContext _context) => new NeqOpExpr(Get(_context.lExpr), Get(_context.rExpr));
        public override ASTBase VisitNullableMemOpExpr([NotNull] VooDoParser.NullableMemOpExprContext _context) => new MemberOpExpr(Get(_context.srcExpr), Get(_context.memberExpr), true);
        public override ASTBase VisitNullCoalOpExpr([NotNull] VooDoParser.NullCoalOpExprContext _context) => new NullCoalesceOpExpr(Get(_context.srcExpr), Get(_context.elseExpr));
        public override ASTBase VisitNullLitExpr([NotNull] VooDoParser.NullLitExprContext _context) => new NullLitExpr();
        public override ASTBase VisitOctIntLitExpr([NotNull] VooDoParser.OctIntLitExprContext _context) => new IntLitExpr(Convert.ToInt32(_context.value.Text.Substring(1), 8));
        public override ASTBase VisitPosOpExpr([NotNull] VooDoParser.PosOpExprContext _context) => new PosOpExpr(Get(_context.srcExpr));
        public override ASTBase VisitRealLitExpr([NotNull] VooDoParser.RealLitExprContext _context) => new RealLitExpr(Convert.ToDouble(_context.value.Text));
        public override ASTBase VisitSequenceStat([NotNull] VooDoParser.SequenceStatContext _context) => new SequenceStat(Get(_context._stats));
        public override ASTBase VisitStringLitExpr([NotNull] VooDoParser.StringLitExprContext _context)
        {
            string raw = _context.value.Text;
            return new StringLitExpr(Syntax.UnescapeString(raw.Substring(1, raw.Length - 2)));
        }
        public override ASTBase VisitSubOpExpr([NotNull] VooDoParser.SubOpExprContext _context) => new SubOpExpr(Get(_context.lExpr), Get(_context.rExpr));
        public override ASTBase VisitSumOpExpr([NotNull] VooDoParser.SumOpExprContext _context) => new SumOpExpr(Get(_context.lExpr), Get(_context.rExpr));
        public override ASTBase VisitWhileStat([NotNull] VooDoParser.WhileStatContext _context) => new WhileStat(Get(_context.condExpr), Get(_context.doStat));
        public override ASTBase VisitNullableIndexExpr([NotNull] VooDoParser.NullableIndexExprContext _context) => new IndexExpr(Get(_context.srcExpr), Get(_context._argsExpr), true);
        public override ASTBase VisitIsExpr([NotNull] VooDoParser.IsExprContext _context) => new IsExpr(Get(_context.srcExpr), Get(_context.typeExpr));
    }

}
