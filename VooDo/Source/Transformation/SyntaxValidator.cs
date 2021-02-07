using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace VooDo.Transformation
{

    public static class SyntaxValidator
    {

        private sealed class Walker : CSharpSyntaxWalker
        {

            private readonly List<Diagnostic> m_diagnostics = new List<Diagnostic>();
            public ImmutableArray<Diagnostic> Diagnostics => m_diagnostics.ToImmutableArray();

            private void EmitDiagnostic(SyntaxNode _node)
                => m_diagnostics.Add(DiagnosticFactory.ForbiddenSyntax(_node));

            private int m_nestedAssignments;

            public override void VisitAssignmentExpression(AssignmentExpressionSyntax _node)
            {
                if (m_nestedAssignments > 0)
                {
                    m_diagnostics.Add(DiagnosticFactory.NestedAssignmentExpression(_node));
                }
                m_nestedAssignments++;
                base.VisitAssignmentExpression(_node);
                m_nestedAssignments--;
            }
            public override void VisitAccessorDeclaration(AccessorDeclarationSyntax _node) => EmitDiagnostic(_node);
            public override void VisitAccessorList(AccessorListSyntax _node) => EmitDiagnostic(_node);
            public override void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax _node) => EmitDiagnostic(_node);
            public override void VisitArrowExpressionClause(ArrowExpressionClauseSyntax _node) => EmitDiagnostic(_node);
            public override void VisitAttribute(AttributeSyntax _node) => EmitDiagnostic(_node);
            public override void VisitAttributeArgument(AttributeArgumentSyntax _node) => EmitDiagnostic(_node);
            public override void VisitAttributeArgumentList(AttributeArgumentListSyntax _node) => EmitDiagnostic(_node);
            public override void VisitAttributeList(AttributeListSyntax _node) => EmitDiagnostic(_node);
            public override void VisitAttributeTargetSpecifier(AttributeTargetSpecifierSyntax _node) => EmitDiagnostic(_node);
            public override void VisitAwaitExpression(AwaitExpressionSyntax _node) => EmitDiagnostic(_node);
            public override void VisitBadDirectiveTrivia(BadDirectiveTriviaSyntax _node) => EmitDiagnostic(_node);
            public override void VisitBaseExpression(BaseExpressionSyntax _node) => EmitDiagnostic(_node);
            public override void VisitBaseList(BaseListSyntax _node) => EmitDiagnostic(_node);
            public override void VisitBinaryPattern(BinaryPatternSyntax _node) => EmitDiagnostic(_node);
            public override void VisitBracketedParameterList(BracketedParameterListSyntax _node) => EmitDiagnostic(_node);
            public override void VisitBreakStatement(BreakStatementSyntax _node) => EmitDiagnostic(_node);
            public override void VisitCasePatternSwitchLabel(CasePatternSwitchLabelSyntax _node) => EmitDiagnostic(_node);
            public override void VisitCatchFilterClause(CatchFilterClauseSyntax _node) => EmitDiagnostic(_node);
            public override void VisitClassDeclaration(ClassDeclarationSyntax _node) => EmitDiagnostic(_node);
            public override void VisitClassOrStructConstraint(ClassOrStructConstraintSyntax _node) => EmitDiagnostic(_node);
            public override void VisitConstantPattern(ConstantPatternSyntax _node) => EmitDiagnostic(_node);
            public override void VisitConstructorConstraint(ConstructorConstraintSyntax _node) => EmitDiagnostic(_node);
            public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax _node) => EmitDiagnostic(_node);
            public override void VisitConstructorInitializer(ConstructorInitializerSyntax _node) => EmitDiagnostic(_node);
            public override void VisitContinueStatement(ContinueStatementSyntax _node) => EmitDiagnostic(_node);
            public override void VisitConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax _node) => EmitDiagnostic(_node);
            public override void VisitDeclarationPattern(DeclarationPatternSyntax _node) => EmitDiagnostic(_node);
            public override void VisitDefaultConstraint(DefaultConstraintSyntax _node) => EmitDiagnostic(_node);
            public override void VisitDefineDirectiveTrivia(DefineDirectiveTriviaSyntax _node) => EmitDiagnostic(_node);
            public override void VisitDelegateDeclaration(DelegateDeclarationSyntax _node) => EmitDiagnostic(_node);
            public override void VisitDestructorDeclaration(DestructorDeclarationSyntax _node) => EmitDiagnostic(_node);
            public override void VisitDiscardPattern(DiscardPatternSyntax _node) => EmitDiagnostic(_node);
            public override void VisitDoStatement(DoStatementSyntax _node) => EmitDiagnostic(_node);
            public override void VisitElifDirectiveTrivia(ElifDirectiveTriviaSyntax _node) => EmitDiagnostic(_node);
            public override void VisitElseDirectiveTrivia(ElseDirectiveTriviaSyntax _node) => EmitDiagnostic(_node);
            public override void VisitEndIfDirectiveTrivia(EndIfDirectiveTriviaSyntax _node) => EmitDiagnostic(_node);
            public override void VisitEndRegionDirectiveTrivia(EndRegionDirectiveTriviaSyntax _node) => EmitDiagnostic(_node);
            public override void VisitErrorDirectiveTrivia(ErrorDirectiveTriviaSyntax _node) => EmitDiagnostic(_node);
            public override void VisitEventDeclaration(EventDeclarationSyntax _node) => EmitDiagnostic(_node);
            public override void VisitEventFieldDeclaration(EventFieldDeclarationSyntax _node) => EmitDiagnostic(_node);
            public override void VisitExplicitInterfaceSpecifier(ExplicitInterfaceSpecifierSyntax _node) => EmitDiagnostic(_node);
            public override void VisitExternAliasDirective(ExternAliasDirectiveSyntax _node) => EmitDiagnostic(_node);
            public override void VisitFieldDeclaration(FieldDeclarationSyntax _node) => EmitDiagnostic(_node);
            public override void VisitFixedStatement(FixedStatementSyntax _node) => EmitDiagnostic(_node);
            public override void VisitForEachStatement(ForEachStatementSyntax _node) => EmitDiagnostic(_node);
            public override void VisitForEachVariableStatement(ForEachVariableStatementSyntax _node) => EmitDiagnostic(_node);
            public override void VisitFromClause(FromClauseSyntax _node) => EmitDiagnostic(_node);
            public override void VisitFunctionPointerCallingConvention(FunctionPointerCallingConventionSyntax _node) => EmitDiagnostic(_node);
            public override void VisitFunctionPointerParameter(FunctionPointerParameterSyntax _node) => EmitDiagnostic(_node);
            public override void VisitFunctionPointerParameterList(FunctionPointerParameterListSyntax _node) => EmitDiagnostic(_node);
            public override void VisitFunctionPointerType(FunctionPointerTypeSyntax _node) => EmitDiagnostic(_node);
            public override void VisitFunctionPointerUnmanagedCallingConvention(FunctionPointerUnmanagedCallingConventionSyntax _node) => EmitDiagnostic(_node);
            public override void VisitFunctionPointerUnmanagedCallingConventionList(FunctionPointerUnmanagedCallingConventionListSyntax _node) => EmitDiagnostic(_node);
            public override void VisitGotoStatement(GotoStatementSyntax _node) => EmitDiagnostic(_node);
            public override void VisitGroupClause(GroupClauseSyntax _node) => EmitDiagnostic(_node);
            public override void VisitIfDirectiveTrivia(IfDirectiveTriviaSyntax _node) => EmitDiagnostic(_node);
            public override void VisitImplicitElementAccess(ImplicitElementAccessSyntax _node) => EmitDiagnostic(_node);
            public override void VisitImplicitStackAllocArrayCreationExpression(ImplicitStackAllocArrayCreationExpressionSyntax _node) => EmitDiagnostic(_node);
            public override void VisitIncompleteMember(IncompleteMemberSyntax _node) => EmitDiagnostic(_node);
            public override void VisitIndexerDeclaration(IndexerDeclarationSyntax _node) => EmitDiagnostic(_node);
            public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax _node) => EmitDiagnostic(_node);
            public override void VisitJoinClause(JoinClauseSyntax _node) => EmitDiagnostic(_node);
            public override void VisitJoinIntoClause(JoinIntoClauseSyntax _node) => EmitDiagnostic(_node);
            public override void VisitLabeledStatement(LabeledStatementSyntax _node) => EmitDiagnostic(_node);
            public override void VisitLetClause(LetClauseSyntax _node) => EmitDiagnostic(_node);
            public override void VisitLineDirectiveTrivia(LineDirectiveTriviaSyntax _node) => EmitDiagnostic(_node);
            public override void VisitLoadDirectiveTrivia(LoadDirectiveTriviaSyntax _node) => EmitDiagnostic(_node);
            public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax _node) => EmitDiagnostic(_node);
            public override void VisitLockStatement(LockStatementSyntax _node) => EmitDiagnostic(_node);
            public override void VisitMakeRefExpression(MakeRefExpressionSyntax _node) => EmitDiagnostic(_node);
            public override void VisitMethodDeclaration(MethodDeclarationSyntax _node) => EmitDiagnostic(_node);
            public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax _node) => EmitDiagnostic(_node);
            public override void VisitNullableDirectiveTrivia(NullableDirectiveTriviaSyntax _node) => EmitDiagnostic(_node);
            public override void VisitOperatorDeclaration(OperatorDeclarationSyntax _node) => EmitDiagnostic(_node);
            public override void VisitOrderByClause(OrderByClauseSyntax _node) => EmitDiagnostic(_node);
            public override void VisitOrdering(OrderingSyntax _node) => EmitDiagnostic(_node);
            public override void VisitParameter(ParameterSyntax _node) => EmitDiagnostic(_node);
            public override void VisitParameterList(ParameterListSyntax _node) => EmitDiagnostic(_node);
            public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax _node) => EmitDiagnostic(_node);
            public override void VisitParenthesizedPattern(ParenthesizedPatternSyntax _node) => EmitDiagnostic(_node);
            public override void VisitPointerType(PointerTypeSyntax _node) => EmitDiagnostic(_node);
            public override void VisitPositionalPatternClause(PositionalPatternClauseSyntax _node) => EmitDiagnostic(_node);
            public override void VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax _node) => EmitDiagnostic(_node);
            public override void VisitPragmaChecksumDirectiveTrivia(PragmaChecksumDirectiveTriviaSyntax _node) => EmitDiagnostic(_node);
            public override void VisitPragmaWarningDirectiveTrivia(PragmaWarningDirectiveTriviaSyntax _node) => EmitDiagnostic(_node);
            public override void VisitPrimaryConstructorBaseType(PrimaryConstructorBaseTypeSyntax _node) => EmitDiagnostic(_node);
            public override void VisitPropertyDeclaration(PropertyDeclarationSyntax _node) => EmitDiagnostic(_node);
            public override void VisitPropertyPatternClause(PropertyPatternClauseSyntax _node) => EmitDiagnostic(_node);
            public override void VisitQueryBody(QueryBodySyntax _node) => EmitDiagnostic(_node);
            public override void VisitQueryContinuation(QueryContinuationSyntax _node) => EmitDiagnostic(_node);
            public override void VisitQueryExpression(QueryExpressionSyntax _node) => EmitDiagnostic(_node);
            public override void VisitRangeExpression(RangeExpressionSyntax _node) => EmitDiagnostic(_node);
            public override void VisitRecordDeclaration(RecordDeclarationSyntax _node) => EmitDiagnostic(_node);
            public override void VisitRecursivePattern(RecursivePatternSyntax _node) => EmitDiagnostic(_node);
            public override void VisitReferenceDirectiveTrivia(ReferenceDirectiveTriviaSyntax _node) => EmitDiagnostic(_node);
            public override void VisitRefExpression(RefExpressionSyntax _node) => EmitDiagnostic(_node);
            public override void VisitRefType(RefTypeSyntax _node) => EmitDiagnostic(_node);
            public override void VisitRefTypeExpression(RefTypeExpressionSyntax _node) => EmitDiagnostic(_node);
            public override void VisitRefValueExpression(RefValueExpressionSyntax _node) => EmitDiagnostic(_node);
            public override void VisitRegionDirectiveTrivia(RegionDirectiveTriviaSyntax _node) => EmitDiagnostic(_node);
            public override void VisitRelationalPattern(RelationalPatternSyntax _node) => EmitDiagnostic(_node);
            public override void VisitSelectClause(SelectClauseSyntax _node) => EmitDiagnostic(_node);
            public override void VisitShebangDirectiveTrivia(ShebangDirectiveTriviaSyntax _node) => EmitDiagnostic(_node);
            public override void VisitSimpleBaseType(SimpleBaseTypeSyntax _node) => EmitDiagnostic(_node);
            public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax _node) => EmitDiagnostic(_node);
            public override void VisitSizeOfExpression(SizeOfExpressionSyntax _node) => EmitDiagnostic(_node);
            public override void VisitStackAllocArrayCreationExpression(StackAllocArrayCreationExpressionSyntax _node) => EmitDiagnostic(_node);
            public override void VisitStructDeclaration(StructDeclarationSyntax _node) => EmitDiagnostic(_node);
            public override void VisitSubpattern(SubpatternSyntax _node) => EmitDiagnostic(_node);
            public override void VisitSwitchExpression(SwitchExpressionSyntax _node) => EmitDiagnostic(_node);
            public override void VisitSwitchExpressionArm(SwitchExpressionArmSyntax _node) => EmitDiagnostic(_node);
            public override void VisitThisExpression(ThisExpressionSyntax _node) => EmitDiagnostic(_node);
            public override void VisitTypeConstraint(TypeConstraintSyntax _node) => EmitDiagnostic(_node);
            public override void VisitTypeParameterConstraintClause(TypeParameterConstraintClauseSyntax _node) => EmitDiagnostic(_node);
            public override void VisitTypePattern(TypePatternSyntax _node) => EmitDiagnostic(_node);
            public override void VisitUnaryPattern(UnaryPatternSyntax _node) => EmitDiagnostic(_node);
            public override void VisitUndefDirectiveTrivia(UndefDirectiveTriviaSyntax _node) => EmitDiagnostic(_node);
            public override void VisitVarPattern(VarPatternSyntax _node) => EmitDiagnostic(_node);
            public override void VisitWarningDirectiveTrivia(WarningDirectiveTriviaSyntax _node) => EmitDiagnostic(_node);
            public override void VisitWhenClause(WhenClauseSyntax _node) => EmitDiagnostic(_node);
            public override void VisitWhereClause(WhereClauseSyntax _node) => EmitDiagnostic(_node);
            public override void VisitWhileStatement(WhileStatementSyntax _node) => EmitDiagnostic(_node);
            public override void VisitYieldStatement(YieldStatementSyntax _node) => EmitDiagnostic(_node);

        }

        public static ImmutableArray<Diagnostic> Validate(SyntaxNode _body)
        {
            if (_body is null)
            {
                throw new ArgumentNullException(nameof(_body));
            }
            Walker walker = new Walker();
            walker.Visit(_body);
            return walker.Diagnostics;
        }

    }

}
