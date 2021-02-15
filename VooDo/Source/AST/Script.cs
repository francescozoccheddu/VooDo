using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST.Directives;
using VooDo.AST.Statements;
using VooDo.Compiling;
using VooDo.Compiling.Emission;
using VooDo.Runtime;
using VooDo.Utils;

using static VooDo.Compiling.Emission.Scope;

using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SFH = VooDo.Utils.SyntaxFactoryHelper;

namespace VooDo.AST
{

    public sealed record Script(ImmutableArray<UsingDirective> Usings, ImmutableArray<Statement> Statements) : Node
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

        public override Script ReplaceNodes(Func<Node?, Node?> _map)
        {
            ImmutableArray<UsingDirective> newUsings = Usings.Map(_map).NonNull();
            ImmutableArray<Statement> newStatements = Statements.Map(_map).NonNull();
            if (newUsings == Usings && newStatements == Statements)
            {
                return this;
            }
            else
            {
                return this with
                {
                    Usings = newUsings,
                    Statements = newStatements
                };
            }
        }

        internal (CompilationUnitSyntax, ImmutableArray<GlobalDefinition>) EmitNode(Session _session)
        {
            Scope scope = new Scope();
            Tagger tagger = _session.Tagger;
            TypeSyntax? returnType = _session.Compilation.Options.ReturnType?.EmitNode(scope, tagger);
            TypeSyntax variableType = SFH.VariableType();
            IEnumerable<ExternAliasDirectiveSyntax> aliases =
                _session.Compilation.Options.References
                .SelectMany(_r => _r.Aliases)
                .Select(_r => SF.ExternAliasDirective(_r).Own(tagger, _r));
            IEnumerable<UsingDirectiveSyntax> usings = Usings.Select(_u => _u.EmitNode(scope, tagger));
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
                                SF.Block(
                                    Statements.SelectMany(
                                        _s => _s is GlobalStatement global
                                            ? global.EmitNode(scope, tagger).Statements
                                            : _s.EmitNode(scope, tagger).ToSyntaxList())));

            ImmutableArray<Scope.GlobalDefinition> globals = scope.GetGlobalDefinitions();
            VariableDeclarationSyntax EmitGlobalDeclaration(Scope.GlobalDefinition _definition)
            {
                TypeSyntax? type = _definition.Prototype.Global.Type.IsVar
                    ? null
                    : _definition.Prototype.Global.Type.EmitNode(scope, tagger);
                return SF.VariableDeclaration(
                            SFH.VariableType(type).Own(tagger, _definition.Prototype.Global.Type),
                            SF.VariableDeclarator(
                                _definition.Identifier,
                                null,
                                SFH.CreateVariableInvocation(
                                    type,
                                    _definition.Prototype.Global.IsAnonymous
                                    ? SF.LiteralExpression(
                                        SyntaxKind.NullLiteralExpression)
                                    : SF.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SF.Literal(_definition.Prototype.Global.Name!)),
                                    _definition.Prototype.Global.HasInitializer
                                    ? _definition.Prototype.Global.Initializer!.EmitNode(new Scope(), tagger)
                                    : SF.LiteralExpression(
                                        SyntaxKind.DefaultLiteralExpression))
                                .ToEqualsValueClause())
                            .ToSeparatedList())
                    .Own(tagger, _definition.Prototype.Source);
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
                SF.ClassDeclaration(CompilationConstants.generatedClassName)
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
            CompilationUnitSyntax root
                = SF.CompilationUnit(
                    aliases.ToSyntaxList(),
                    usings.ToSyntaxList(),
                    SF.List<AttributeListSyntax>(),
                    classDeclaration.ToSyntaxList<MemberDeclarationSyntax>())
                .Own(tagger, this);
            return (root, globals);
        }

        public override IEnumerable<BodyNode> Children => ((IEnumerable<BodyNode>) Usings).Concat(Statements);
        public override string ToString() => (string.Join('\n', Usings) + "\n\n" + string.Join('\n', Statements)).Trim();

        #endregion

    }

}
