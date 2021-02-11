using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Compilation;
using VooDo.Language.AST.Directives;
using VooDo.Language.AST.Names;
using VooDo.Language.AST.Statements;
using VooDo.Language.Linking;
using VooDo.Runtime;
using VooDo.Utils;

using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SFH = VooDo.Utils.SyntaxFactoryHelper;

namespace VooDo.Language.AST
{

    public sealed record Script(ImmutableArray<UsingDirective> Usings, BlockStatement Body) : Node
    {

        #region Members

        private ImmutableArray<UsingDirective> m_usings = Usings.EmptyIfDefault();
        public ImmutableArray<UsingDirective> Usings
        {
            get => m_usings;
            init => m_usings = value.EmptyIfDefault();
        }

        #endregion

        #region Overrides

        internal override CompilationUnitSyntax EmitNode(Scope _scope, Marker _marker)
            => EmitNode(_scope, _marker, ImmutableArray.Create(new Identifier(Compiler.runtimeReferenceExternAlias)), null);

        internal CompilationUnitSyntax EmitNode(Scope _scope, Marker _marker, ImmutableArray<Identifier> _externAliases, ComplexType? _returnType)
        {
            TypeSyntax? returnType = _returnType?.EmitNode(_scope, _marker);
            TypeSyntax variableType = SFH.VariableType();
            IEnumerable<ExternAliasDirectiveSyntax> aliases = _externAliases
                .EmptyIfDefault()
                .Select(_i => SF.ExternAliasDirective(_i).Own(_marker, _i));
            IEnumerable<UsingDirectiveSyntax> usings = Usings.Select(_u => _u.EmitNode(_scope, _marker));
            MethodDeclarationSyntax? runMethod = SF.MethodDeclaration(
                                returnType ?? SFH.Void(),
                                SF.Identifier(returnType is null
                                    ? nameof(Program.Run)
                                    : nameof(Program<object>.TypedRun)))
                            .WithModifiers(
                                SFH.Tokens(
                                    SyntaxKind.ProtectedKeyword,
                                    SyntaxKind.OverrideKeyword))
                            .WithBody(
                                Body.EmitNode(_scope, _marker));

            ImmutableArray<Scope.GlobalDefinition> globals = _scope.GetGlobalDefinitions();
            VariableDeclarationSyntax EmitGlobalDeclaration(Scope.GlobalDefinition _definition)
            {
                TypeSyntax? type = _definition.Global.Type.IsVar
                    ? null
                    : _definition.Global.Type.EmitNode(_scope, _marker);
                return SF.VariableDeclaration(
                            SFH.VariableType(type),
                            SF.VariableDeclarator(
                                _definition.Identifier,
                                null,
                                SFH.CreateVariableInvocation(
                                    type,
                                    _definition.Global.IsAnonymous
                                    ? SF.LiteralExpression(
                                        SyntaxKind.NullLiteralExpression)
                                    : SF.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SF.Literal(_definition.Global.Name!)),
                                    _definition.Global.HasInitializer
                                    ? _definition.Global.Initializer!.EmitNode(_scope, _marker)
                                    : SF.LiteralExpression(
                                        SyntaxKind.DefaultLiteralExpression))
                                .ToEqualsValueClause())
                            .ToSeparatedList());
            }
            PropertyDeclarationSyntax variablesProperty = SFH.ArrowProperty(
                SF.ArrayType(
                    SFH.VariableType(),
                    SFH.SingleArrayRank()),
                nameof(Program.m_Variables),
                SF.ArrayCreationExpression(
                    SF.ArrayType(variableType)
                    .WithRankSpecifiers(
                        SFH.SingleArrayRank()))
                    .WithInitializer(
                        SF.InitializerExpression(
                            SyntaxKind.ArrayInitializerExpression,
                            globals.Select(_g => SFH.ThisMemberAccess(_g.Identifier))
                        .ToSeparatedList<ExpressionSyntax>())))
                .WithModifiers(
                    SFH.Tokens(
                        SyntaxKind.ProtectedKeyword,
                        SyntaxKind.OverrideKeyword));
            IEnumerable<FieldDeclarationSyntax> globalDeclarations =
                globals.Select(_g =>
                    SF.FieldDeclaration(
                        SF.List<AttributeListSyntax>(),
                        SFH.Tokens(
                            SyntaxKind.PrivateKeyword,
                            SyntaxKind.ReadOnlyKeyword),
                        EmitGlobalDeclaration(_g)));
            ClassDeclarationSyntax? classDeclaration =
                SF.ClassDeclaration(Compiler.generatedClassName)
                    .WithModifiers(
                        SFH.Tokens(
                            SyntaxKind.PublicKeyword,
                            SyntaxKind.SealedKeyword))
                    .WithBaseList(
                        SF.BaseList(
                            SF.SimpleBaseType(
                                SFH.ProgramType(returnType))
                            .ToSeparatedList<BaseTypeSyntax>()))
                    .WithMembers(globalDeclarations
                        .Cast<MemberDeclarationSyntax>()
                        .Append(variablesProperty)
                        .Append(runMethod)
                        .ToSyntaxList());
            return SF.CompilationUnit(
                    aliases.ToSyntaxList(),
                    usings.ToSyntaxList(),
                    SF.List<AttributeListSyntax>(),
                    classDeclaration.ToSyntaxList<MemberDeclarationSyntax>())
                .Own(_marker, this);
        }

        public override IEnumerable<Node> Children => ((IEnumerable<Node>) Usings).Append(Body);
        public override string ToString() => Usings.Aggregate("", (_a, _u) => $"{_a}{_u}\n") + Body;

        #endregion

    }

}
