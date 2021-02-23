using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST;
using VooDo.AST.Directives;
using VooDo.AST.Expressions;
using VooDo.AST.Names;
using VooDo.AST.Statements;
using VooDo.Problems;
using VooDo.Runtime;
using VooDo.Utils;

using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SK = Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using SU = VooDo.Utils.SyntaxFactoryUtils;

namespace VooDo.Compiling.Emission
{
    internal sealed class Emitter
    {

        internal static (CompilationUnitSyntax syntax, ImmutableArray<Scope.GlobalDefinition> globals) Emit(Script _script, Session _session)
        {
            if (_script.Root != _script)
            {
                throw new ArgumentException("Script is not root", nameof(_script));
            }
            Emitter emitter = new Emitter(_session.Tagger);
            CompilationUnitSyntax syntax = emitter.EmitScript(_script, _session);
            return (syntax, emitter.m_scope.GetGlobalDefinitions());
        }

        internal static UsingDirectiveSyntax Emit(UsingDirective _node, Tagger _tagger) => new Emitter(_tagger).EmitUsingDirective(_node);
        internal static TypeSyntax Emit(ComplexType _node, Tagger _tagger) => new Emitter(_tagger).EmitComplexType(_node);
        internal static ExpressionSyntax Emit(Expression _node, Tagger _tagger) => new Emitter(_tagger).EmitExpression(_node);
        internal static NameSyntax Emit(Namespace _node, Tagger _tagger) => new Emitter(_tagger).EmitNamespace(_node);

        private static readonly ImmutableDictionary<string, SyntaxToken> s_predefinedTypesTokens =
            new SK[] {
                        SK.BoolKeyword,
                        SK.CharKeyword,
                        SK.StringKeyword,
                        SK.ByteKeyword,
                        SK.SByteKeyword,
                        SK.ShortKeyword,
                        SK.UShortKeyword,
                        SK.IntKeyword,
                        SK.UIntKeyword,
                        SK.LongKeyword,
                        SK.ULongKeyword,
                        SK.DecimalKeyword,
                        SK.FloatKeyword,
                        SK.DoubleKeyword,
                        SK.ObjectKeyword
            }.Select(_k => SF.Token(_k))
            .ToImmutableDictionary(_t => _t.ValueText);

        private readonly Tagger m_tagger;
        private Scope m_scope;

        private Emitter(Tagger _tagger)
        {
            m_scope = new Scope();
            m_tagger = _tagger;
        }

        private ExpressionSyntax EmitExpression(Expression _node) => _node switch
        {
            ArrayCreationExpression e => EmitArrayCreation(e),
            AsExpression e => EmitAsExpression(e),
            BinaryExpression e => EmitBinaryExpressions(e),
            CastExpression e => EmitCastExpression(e),
            ConditionalExpression e => EmitConditionalExpression(e),
            DefaultExpression e => EmitDefaultExpression(e),
            ElementAccessExpression e => EmitElementAccessExpression(e),
            GlobalExpression e => EmitGlobalExpression(e),
            InvocationExpression e => EmitInvocation(e),
            IsExpression e => EmitIsExpression(e),
            LiteralExpression e => EmitLiteralExpression(e),
            MemberAccessExpression e => EmitMemberAccessExpression(e),
            NameExpression e => EmitNameExpression(e),
            ObjectCreationExpression e => EmitObjectCreation(e),
            TupleExpression e => EmitTupleExpression(e),
            TupleDeclarationExpression e => EmitTupleDeclarationExpression(e),
            UnaryExpression e => EmitUnaryExpression(e)
        };

        private ExpressionSyntax EmitComplexTypeOrExpression(ComplexTypeOrExpression _node) => _node switch
        {
            Expression e => EmitExpression(e),
            ComplexType t => EmitComplexType(t)
        };

        private IEnumerable<StatementSyntax> EmitStatements(Statement _node) => _node switch
        {
            AssignmentStatement s => EnumerableExtensions.Singleton(EmitAssignmentStatement(s)),
            BlockStatement s => EnumerableExtensions.Singleton(EmitBlockStatement(s)),
            DeclarationStatement s => EmitDeclarationStatement(s),
            ExpressionStatement s => EnumerableExtensions.Singleton(EmitExpressionStatement(s)),
            GlobalStatement s => s.SelectMany(EmitDeclarationStatement),
            IfStatement s => EnumerableExtensions.Singleton(EmitIfStatement(s)),
            ReturnStatement s => EnumerableExtensions.Singleton((StatementSyntax) EmitReturnStatement(s)),
        };

        private StatementSyntax EmitStatement(Statement _node)
        {
            ImmutableArray<StatementSyntax> statements = EmitStatements(_node).ToImmutableArray();
            return statements.Length switch
            {
                > 1 => SF.Block(statements),
                _ => statements.Single()
            };
        }

        private UsingDirectiveSyntax EmitUsingDirective(UsingDirective _node) => _node switch
        {
            UsingNamespaceDirective u => EmitUsingNamespaceDirective(u),
            UsingStaticDirective u => EmitUsingStaticDirective(u)
        };

        private ArrayRankSpecifierSyntax EmitComplexTypeRankSpecifier(ComplexType.RankSpecifier _node)
            => SU.ArrayRank(_node.Rank).Own(m_tagger, _node);

        private TypeSyntax EmitComplexType(ComplexType _node)
        {
            TypeSyntax type = _node switch
            {
                QualifiedType t => EmitNonArrayNonNullableQualifiedType(t),
                TupleType t => EmitNonArrayNonNullableTupleType(t)
            };
            if (_node.IsNullable)
            {
                type = SF.NullableType(type);
            }
            if (_node.IsArray)
            {
                type = SF.ArrayType(type, _node.Ranks.Select(EmitComplexTypeRankSpecifier).ToSyntaxList());
            }
            return type.Own(m_tagger, _node);
        }

        private TypeSyntax EmitComplexTypeOrVar(ComplexTypeOrVar _node)
            => _node.IsVar
                ? SF.IdentifierName("var")
                : EmitComplexType(_node.Type!)
            .Own(m_tagger, _node);

        private SyntaxToken EmitIdentifier(Identifier _node)
            => SF.Identifier(_node).Own(m_tagger, _node);

        private VariableDesignationSyntax EmitIdentifierOrDiscard(IdentifierOrDiscard _node)
            => (_node.IsDiscard
                ? (VariableDesignationSyntax) SF.DiscardDesignation()
                : SF.SingleVariableDesignation(EmitIdentifier(_node.Identifier!)))
            .Own(m_tagger, _node);

        private NameSyntax EmitNamespace(Namespace _node)
        {
            IdentifierNameSyntax[] path = _node.Path.Select(_i => SF.IdentifierName(EmitIdentifier(_i)).Own(m_tagger, _i)).ToArray();
            NameSyntax type = path[0];
            if (_node.IsAliasQualified)
            {
                type = SF.AliasQualifiedName(
                    SF.IdentifierName(EmitIdentifier(_node.Alias!)),
                    (SimpleNameSyntax) type);
            }
            foreach (SimpleType name in _node.Path.Skip(1))
            {
                type = SF.QualifiedName(type, (SimpleNameSyntax) EmitSimpleType(name));
            }
            return type.Own(m_tagger, _node);
        }

        private TypeSyntax EmitNonArrayNonNullableQualifiedType(QualifiedType _node)
        {
            if (_node.IsSimple)
            {
                return EmitSimpleType(_node.Path[0]);
            }
            NameSyntax type = (NameSyntax) EmitSimpleType(_node.Path[0]);
            if (_node.IsAliasQualified)
            {
                type = SF.AliasQualifiedName(
                    SF.IdentifierName(EmitIdentifier(_node.Alias!)).Own(m_tagger, _node.Alias!),
                    (SimpleNameSyntax) type);
            }
            foreach (SimpleType name in _node.Path.Skip(1))
            {
                type = SF.QualifiedName(type, (SimpleNameSyntax) EmitSimpleType(name));
            }
            return type.Own(m_tagger, _node);
        }

        private TypeSyntax EmitSimpleType(SimpleType _node)
        {
            if (_node.Parent is not null
                && ((QualifiedType) _node.Parent).IsSimple
                && !_node.IsGeneric
                && s_predefinedTypesTokens.TryGetValue(_node.Name, out SyntaxToken token))
            {
                return SF.PredefinedType(token).Own(m_tagger, _node);
            }
            return (_node.IsGeneric
                    ? (SimpleNameSyntax) SU.GenericName(
                            EmitIdentifier(_node.Name),
                            _node.TypeArguments.Select(EmitComplexType))
                    : SF.IdentifierName(EmitIdentifier(_node.Name)))
                .Own(m_tagger, _node);
        }

        private TupleElementSyntax EmitTupleTypeElement(TupleType.Element _node)
            => SF.TupleElement(
                    EmitComplexType(_node.Type),
                    _node.IsNamed
                        ? EmitIdentifier(_node.Name!)
                        : SF.Token(SK.None))
                .Own(m_tagger, _node);

        private TupleTypeSyntax EmitNonArrayNonNullableTupleType(TupleType _node)
            => SF.TupleType(_node.Select(EmitTupleTypeElement).ToSeparatedList()).Own(m_tagger, _node);

        private UsingDirectiveSyntax EmitUsingNamespaceDirective(UsingNamespaceDirective _node)
        {
            NameSyntax name = EmitNamespace(_node.Namespace);
            UsingDirectiveSyntax result;
            if (_node.HasAlias)
            {
                SyntaxToken alias = EmitIdentifier(_node.Alias!);
                NameEqualsSyntax aliasName = SF.NameEquals(SF.IdentifierName(alias)).Own(m_tagger, _node.Alias!);
                result = SF.UsingDirective(aliasName, name);
            }
            else
            {
                result = SF.UsingDirective(name);
            }
            return result.Own(m_tagger, _node);
        }

        private UsingDirectiveSyntax EmitUsingStaticDirective(UsingStaticDirective _node)
            => SF.UsingDirective(
                    SF.Token(SK.StaticKeyword),
                    null,
                    (NameSyntax) EmitNonArrayNonNullableQualifiedType(_node.Type))
                .Own(m_tagger, _node);

        private ExpressionStatementSyntax EmitAssignmentStatement(AssignmentStatement _node)
            => SF.ExpressionStatement(
                SF.AssignmentExpression(
                    _node.Kind switch
                    {
                        AssignmentStatement.EKind.Simple => SK.SimpleAssignmentExpression,
                        AssignmentStatement.EKind.Add => SK.AddAssignmentExpression,
                        AssignmentStatement.EKind.Subtract => SK.SubtractAssignmentExpression,
                        AssignmentStatement.EKind.Multiply => SK.MultiplyAssignmentExpression,
                        AssignmentStatement.EKind.Divide => SK.DivideAssignmentExpression,
                        AssignmentStatement.EKind.Modulo => SK.ModuloAssignmentExpression,
                        AssignmentStatement.EKind.LeftShift => SK.LeftShiftAssignmentExpression,
                        AssignmentStatement.EKind.RightShift => SK.RightShiftAssignmentExpression,
                        AssignmentStatement.EKind.BitwiseAnd => SK.AndAssignmentExpression,
                        AssignmentStatement.EKind.BitwiseOr => SK.OrAssignmentExpression,
                        AssignmentStatement.EKind.BitwiseXor => SK.ExclusiveOrAssignmentExpression,
                        AssignmentStatement.EKind.Coalesce => SK.CoalesceAssignmentExpression,
                    },
                     EmitExpression(_node.Target),
                     EmitExpression(_node.Source)))
            .Own(m_tagger, _node);

        private BlockSyntax EmitBlockStatement(BlockStatement _node)
        {
            Scope oldScope = m_scope;
            m_scope = m_scope.CreateNested();
            BlockSyntax block = SF.Block(_node.SelectMany(EmitStatements).ToSyntaxList()).Own(m_tagger, _node);
            m_scope = oldScope;
            return block;
        }

        private IEnumerable<StatementSyntax> EmitDeclarationStatement(DeclarationStatement _node)
        {
            TypeSyntax type = EmitComplexTypeOrVar(_node.Type);
            if (_node.Parent is GlobalStatement && !_node.Type.IsVar)
            {
                type = SU.VariableType(type);
            }
            return _node.Declarators.Select(_d =>
                SF.LocalDeclarationStatement(
                        SF.VariableDeclaration(type, EmitDeclarationStatementDeclarator(_d).ToSeparatedList()))
                .Own(m_tagger, _node));
        }

        private VariableDeclaratorSyntax EmitDeclarationStatementDeclarator(DeclarationStatement.Declarator _node)
        {
            ExpressionSyntax? initializer;
            if (_node.Parent is not null && _node.Parent.Parent is GlobalStatement globalStatement)
            {
                Global global = new Global(globalStatement.IsConstant, ((DeclarationStatement) _node.Parent).Type, _node.Name, _node.Initializer);
                Scope.GlobalDefinition globalDefinition = m_scope.AddGlobal(new GlobalPrototype(global, _node));
                initializer = SU.ThisMemberAccess(globalDefinition.Identifier);
            }
            else
            {
                m_scope.AddLocal(_node, _node.Name);
                initializer = _node.Initializer is null ? null : EmitExpression(_node.Initializer);
            }
            EqualsValueClauseSyntax? initializerClause = initializer?.ToEqualsValueClause();
            return SF.VariableDeclarator(EmitIdentifier(_node.Name), null, initializerClause).Own(m_tagger, _node);
        }

        private ExpressionStatementSyntax EmitExpressionStatement(ExpressionStatement _node)
            => SF.ExpressionStatement(EmitExpression(_node.Expression)).Own(m_tagger, _node);

        private IfStatementSyntax EmitIfStatement(IfStatement _node)
        {
            ExpressionSyntax condition = EmitExpression(_node.Condition);
            StatementSyntax then = EmitStatement(_node.Then);
            return (_node.HasElse
                ? SF.IfStatement(condition, then, SF.ElseClause(EmitStatement(_node.Else!)))
                : SF.IfStatement(condition, then))
                .Own(m_tagger, _node);
        }

        private ReturnStatementSyntax EmitReturnStatement(ReturnStatement _node)
            => SF.ReturnStatement(EmitExpression(_node.Expression)).Own(m_tagger, _node);

        private ExpressionSyntax EmitOutArgument(OutDeclarationArgument _node)
            => SF.DeclarationExpression(
                    EmitComplexTypeOrVar(_node.Type),
                    EmitIdentifierOrDiscard(_node.Name))
                .Own(m_tagger, _node);

        private ArgumentSyntax EmitArgument(Argument _node)
          => SF.Argument(_node switch
          {
              OutDeclarationArgument a => EmitOutArgument(a),
              ValueArgument a => EmitExpression(a.Expression),
              AssignableArgument a => EmitExpression(a.Expression)
          })
              .WithRefKindKeyword(SF.Token(_node.Kind switch
              {
                  Argument.EKind.Value => SK.None,
                  Argument.EKind.Ref => SK.RefKeyword,
                  Argument.EKind.Out => SK.OutKeyword,
                  Argument.EKind.In => SK.InKeyword
              }))
              .WithNameColon(_node.Parameter is null
                  ? null
                  : SF.NameColon(
                      SF.IdentifierName(EmitIdentifier(_node.Parameter))))
              .Own(m_tagger, _node);

        private BinaryExpressionSyntax EmitAsExpression(AsExpression _node)
            => SF.BinaryExpression(
                SK.AsExpression,
                EmitExpression(_node.Expression),
                EmitComplexType(_node.Type))
            .Own(m_tagger, _node);

        private BinaryExpressionSyntax EmitBinaryExpressions(BinaryExpression _node)
            => SF.BinaryExpression(
                _node.Kind switch
                {
                    BinaryExpression.EKind.Add => SK.AddExpression,
                    BinaryExpression.EKind.Subtract => SK.SubtractExpression,
                    BinaryExpression.EKind.Multiply => SK.MultiplyExpression,
                    BinaryExpression.EKind.Divide => SK.DivideExpression,
                    BinaryExpression.EKind.Modulo => SK.ModuloExpression,
                    BinaryExpression.EKind.LeftShift => SK.LeftShiftExpression,
                    BinaryExpression.EKind.RightShift => SK.RightShiftExpression,
                    BinaryExpression.EKind.Equals => SK.EqualsExpression,
                    BinaryExpression.EKind.NotEquals => SK.NotEqualsExpression,
                    BinaryExpression.EKind.LessThan => SK.LessThanExpression,
                    BinaryExpression.EKind.LessThanOrEqual => SK.LessThanOrEqualExpression,
                    BinaryExpression.EKind.GreaterThan => SK.GreaterThanExpression,
                    BinaryExpression.EKind.GreaterThanOrEqual => SK.GreaterThanOrEqualExpression,
                    BinaryExpression.EKind.Coalesce => SK.CoalesceExpression,
                    BinaryExpression.EKind.LogicAnd => SK.LogicalAndExpression,
                    BinaryExpression.EKind.LogicOr => SK.LogicalOrExpression,
                    BinaryExpression.EKind.BitwiseAnd => SK.BitwiseAndExpression,
                    BinaryExpression.EKind.BitwiseOr => SK.BitwiseOrExpression,
                    BinaryExpression.EKind.BitwiseXor => SK.ExclusiveOrExpression,
                },
                EmitExpression(_node.Left),
                EmitExpression(_node.Right))
            .Own(m_tagger, _node);

        private CastExpressionSyntax EmitCastExpression(CastExpression _node)
            => SF.CastExpression(
                EmitComplexType(_node.Type),
                EmitExpression(_node.Expression))
            .Own(m_tagger, _node);

        private ConditionalExpressionSyntax EmitConditionalExpression(ConditionalExpression _node)
            => SF.ConditionalExpression(
                EmitExpression(_node.Condition),
                EmitExpression(_node.True),
                EmitExpression(_node.False))
            .Own(m_tagger, _node);

        private ExpressionSyntax EmitDefaultExpression(DefaultExpression _node)
            => (_node.HasType
                ? SF.DefaultExpression(EmitComplexType(_node.Type!))
                : (ExpressionSyntax) SF.LiteralExpression(SK.DefaultExpression))
            .Own(m_tagger, _node);

        private ElementAccessExpressionSyntax EmitElementAccessExpression(ElementAccessExpression _node)
            => SF.ElementAccessExpression(
                EmitExpression(_node.Source),
                SU.BracketedArguments(
                        _node.Arguments.Select(_a => SF.Argument(EmitExpression(_a)).Own(m_tagger, _a))))
            .Own(m_tagger, _node);

        private InvocationExpressionSyntax EmitGlobalExpression(GlobalExpression _node)
        {
            Scope.GlobalDefinition globalDefinition = m_scope.AddGlobal(new GlobalPrototype(new Global(false, ComplexTypeOrVar.Var, null, _node.Initializer), _node));
            return SU.SetControllerAndGetValueInvocation(
                    SU.ThisMemberAccess(globalDefinition.Identifier),
                    EmitExpression(_node.Controller).Own(m_tagger, _node.Controller))
                .Own(m_tagger, _node);
        }

        private ExpressionSyntax EmitMethod(InvocationExpression.Method _node)
        {
            TypeArgumentListSyntax typeArgumentList = SU.TypeArguments(_node.TypeArguments.Select(EmitComplexType));
            ExpressionSyntax source = _node.Source switch
            {
                NameExpression s => SF.GenericName(EmitIdentifier(s.Name), typeArgumentList),
                MemberAccessExpression s => SU.MemberAccess(
                    EmitComplexTypeOrExpression(s.Source),
                    SF.GenericName(EmitIdentifier(s.Member), typeArgumentList))
            };
            return source.Own(m_tagger, _node);
        }

        private InvocationExpressionSyntax EmitInvocation(InvocationExpression _node)
            => SU.Invocation(
                _node.Source switch
                {
                    InvocationExpression.Method s => EmitMethod(s),
                    InvocationExpression.SimpleCallable s => EmitExpression(s.Source)
                },
                _node.Arguments.Select(EmitArgument))
            .Own(m_tagger, _node);

        private ExpressionSyntax EmitIsExpression(IsExpression _node)
            => (_node.IsDeclaration
            ? SF.IsPatternExpression(
                EmitExpression(_node.Expression),
                SF.DeclarationPattern(
                    EmitComplexType(_node.Type),
                     EmitIdentifierOrDiscard(_node.Name!)))
            : (ExpressionSyntax) SF.BinaryExpression(
                SK.IsExpression,
                EmitExpression(_node.Expression),
                EmitComplexType(_node.Type)))
            .Own(m_tagger, _node);

        private LiteralExpressionSyntax EmitLiteralExpression(LiteralExpression _node)
        {
            SK kind = _node.Value switch
            {
                true => SK.TrueLiteralExpression,
                false => SK.FalseLiteralExpression,
                null => SK.NullLiteralExpression,
                char => SK.CharacterLiteralExpression,
                string => SK.StringLiteralExpression,
                _ => SK.NumericLiteralExpression
            };
            return (_node.Value is bool or null
                ? SF.LiteralExpression(kind)
                : SF.LiteralExpression(kind, _node.Value switch
                {
                    char v => SF.Literal(v),
                    decimal v => SF.Literal(v),
                    string v => SF.Literal(v),
                    uint v => SF.Literal(v),
                    double v => SF.Literal(v),
                    float v => SF.Literal(v),
                    ulong v => SF.Literal(v),
                    long v => SF.Literal(v),
                    int v => SF.Literal(v)
                })).Own(m_tagger, _node);
        }

        private MemberAccessExpressionSyntax EmitMemberAccessExpression(MemberAccessExpression _node)
            => SU.MemberAccess(
                EmitComplexTypeOrExpression(_node.Source),
                EmitIdentifier(_node.Member).Own(m_tagger, _node.Member))
            .Own(m_tagger, _node);

        private ExpressionSyntax EmitNameExpression(NameExpression _node)
        {
            bool isGlobal = m_scope.IsGlobal(_node.Name);
            bool isConstant = m_scope.IsConstant(_node.Name);
            if (_node.IsControllerOf)
            {
                if (!isGlobal)
                {
                    throw new ControllerOfNonGlobalProblem(_node).AsThrowable();
                }
                if (isConstant)
                {
                    throw new ControllerOfConstantProblem(_node).AsThrowable();
                }
            }
            if (isConstant && _node.Parent is AssignmentStatement assignment && assignment.Target == _node)
            {
                throw new AssignmentOfConstantProblem(_node).AsThrowable();
            }
            ExpressionSyntax result;
            IdentifierNameSyntax name = SF.IdentifierName(EmitIdentifier(_node.Name));
            result = isGlobal
                ? SU.MemberAccess(name, _node.IsControllerOf ? nameof(Variable<object>.ControllerFactory) : nameof(Variable<object>.Value))
                : (ExpressionSyntax) name;
            return result.Own(m_tagger, _node);
        }

        private ExpressionSyntax EmitObjectCreation(ObjectCreationExpression _node)
        {
            ArgumentListSyntax argumentList = SU.Arguments(_node.Arguments.Select(EmitArgument));
            return (_node.IsTypeImplicit
                ? (ExpressionSyntax) SF.ImplicitObjectCreationExpression(argumentList, null)
                : SF.ObjectCreationExpression(EmitComplexType(_node.Type!), argumentList, null))
                .Own(m_tagger, _node);
        }

        private ArrayCreationExpressionSyntax EmitArrayCreation(ArrayCreationExpression _node)
        {
            ArrayTypeSyntax type = (ArrayTypeSyntax) EmitComplexType(_node.Type);
            SyntaxList<ArrayRankSpecifierSyntax> rankSpecifiers = type.RankSpecifiers;
            ArrayRankSpecifierSyntax rank = rankSpecifiers[0].WithSizes(_node.Sizes.Select(EmitExpression).ToSeparatedList());
            rankSpecifiers = new[] { rank }.Concat(rankSpecifiers.Skip(1)).ToSyntaxList();
            type = type.WithRankSpecifiers(rankSpecifiers);
            return SF.ArrayCreationExpression(type).Own(m_tagger, _node);
        }

        private PrefixUnaryExpressionSyntax EmitUnaryExpression(UnaryExpression _node)
            => SF.PrefixUnaryExpression(
                _node.Kind switch
                {
                    UnaryExpression.EKind.Plus => SK.UnaryPlusExpression,
                    UnaryExpression.EKind.Minus => SK.UnaryMinusExpression,
                    UnaryExpression.EKind.LogicNot => SK.LogicalNotExpression,
                    UnaryExpression.EKind.BitwiseNot => SK.BitwiseNotExpression,
                },
                EmitExpression(_node.Expression))
            .Own(m_tagger, _node);

        private TupleExpressionSyntax EmitTupleDeclarationExpression(TupleDeclarationExpression _node)
            => SF.TupleExpression(_node.Select(EmitTupleDeclarationExpressionElement).ToSeparatedList()).Own(m_tagger, _node);

        private TupleExpressionSyntax EmitTupleExpression(TupleExpression _node)
            => SF.TupleExpression(_node.Select(EmitTupleExpressionElement).ToSeparatedList()).Own(m_tagger, _node);

        private ArgumentSyntax EmitTupleExpressionElement(TupleExpression.Element _node)
            => SF.Argument(EmitExpression(_node.Expression))
            .WithNameColon(_node.IsNamed
                ? SF.NameColon(SF.IdentifierName(EmitIdentifier(_node.Name!)).Own(m_tagger, _node.Name!))
                : null)
            .Own(m_tagger, _node);

        private ArgumentSyntax EmitTupleDeclarationExpressionElement(TupleDeclarationExpression.Element _node)
        {
            if (!_node.Name.IsDiscard)
            {
                m_scope.AddLocal(_node, _node.Name.Identifier!);
            }
            return SF.Argument(
                        SF.DeclarationExpression(
                            EmitComplexTypeOrVar(_node.Type),
                            EmitIdentifierOrDiscard(_node.Name).Own(m_tagger, _node.Name)))
                        .Own(m_tagger, _node);
        }

        private CompilationUnitSyntax EmitScript(Script _node, Session _session)
        {
            TypeSyntax? returnType = _session.Compilation.Options.ReturnType is null ? null : EmitComplexType(_session.Compilation.Options.ReturnType);
            TypeSyntax variableType = SU.VariableType();
            IEnumerable<ExternAliasDirectiveSyntax> aliases =
                _session.Compilation.Options.References
                .SelectMany(_r => _r.Aliases)
                .Select(_r => SF.ExternAliasDirective(_r).Own(m_tagger, _r));
            IEnumerable<UsingDirectiveSyntax> usings = _node.Usings.Select(EmitUsingDirective);
            MethodDeclarationSyntax? runMethod = SF.MethodDeclaration(
                                returnType ?? SU.Void(),
                                SF.Identifier(returnType is null
                                    ? nameof(Program.Run)
                                    : nameof(TypedProgram<object>.TypedRun)))
                            .WithModifiers(
                                SU.Tokens(
                                    SK.ProtectedKeyword,
                                    SK.OverrideKeyword))
                            .WithBody(
                                SF.Block(
                                    _node.Statements.SelectMany(EmitStatements)));

            ImmutableArray<Scope.GlobalDefinition> globals = m_scope.GetGlobalDefinitions();
            VariableDeclarationSyntax EmitGlobalDeclaration(Scope.GlobalDefinition _definition)
            {
                Scope oldScope = m_scope;
                m_scope = new Scope();
                TypeSyntax? type = _definition.Prototype.Global.Type.IsVar
                    ? null
                    : EmitComplexTypeOrVar(_definition.Prototype.Global.Type);
                VariableDeclarationSyntax declaration = SF.VariableDeclaration(
                            SU.VariableType(type).Own(m_tagger, _definition.Prototype.Global.Type),
                            SF.VariableDeclarator(
                                _definition.Identifier,
                                null,
                                SU.CreateVariableInvocation(
                                    type,
                                    _definition.Prototype.Global.IsConstant,
                                    _definition.Prototype.Global.Name!,
                                    _definition.Prototype.Global.HasInitializer
                                    ? EmitExpression(_definition.Prototype.Global.Initializer!)
                                    : SF.LiteralExpression(
                                        SK.DefaultLiteralExpression))
                                .ToEqualsValueClause())
                            .ToSeparatedList())
                    .Own(m_tagger, _definition.Prototype.Source);
                m_scope = oldScope;
                return declaration;
            }
            PropertyDeclarationSyntax variablesProperty = SU.ArrowProperty(
                SU.SingleArray(
                    SU.VariableType()),
                nameof(Program.m_Variables),
                SF.ArrayCreationExpression(
                    SU.SingleArray(variableType))
                .WithInitializer(
                    SF.InitializerExpression(
                        SK.ArrayInitializerExpression,
                        globals.Select(_g => SU.ThisMemberAccess(_g.Identifier))
                    .ToSeparatedList<ExpressionSyntax>())))
                .WithModifiers(
                    SU.Tokens(
                        SK.ProtectedKeyword,
                        SK.OverrideKeyword));
            IEnumerable<FieldDeclarationSyntax> globalDeclarations =
                globals.Select(_g =>
                    SF.FieldDeclaration(
                        SF.List<AttributeListSyntax>(),
                        SU.Tokens(
                            SK.PrivateKeyword,
                            SK.ReadOnlyKeyword),
                        EmitGlobalDeclaration(_g)));
            ClassDeclarationSyntax? classDeclaration =
                SF.ClassDeclaration(_session.Compilation.Options.ClassName)
                    .WithModifiers(
                        SU.Tokens(
                            SK.PublicKeyword,
                            SK.SealedKeyword))
                    .WithBaseList(
                        SF.BaseList(
                            SF.SimpleBaseType(
                                SU.ProgramType(returnType))
                            .ToSeparatedList<BaseTypeSyntax>()))
                    .WithMembers(globalDeclarations
                        .Cast<MemberDeclarationSyntax>()
                        .Append(variablesProperty)
                        .Append(runMethod)
                        .ToSyntaxList());
            CompilationUnitSyntax root
                = SF.CompilationUnit(
                    aliases.ToSyntaxList(),
                    usings.ToSyntaxList(),
                    SF.List<AttributeListSyntax>(),
                    SF.NamespaceDeclaration(
                        _session.Compilation.Options.Namespace.ToNameSyntax())
                    .WithMembers(
                        classDeclaration.ToSyntaxList<MemberDeclarationSyntax>())
                    .ToSyntaxList<MemberDeclarationSyntax>())
                .Own(m_tagger, _node);
            return root;
        }

    }

}
