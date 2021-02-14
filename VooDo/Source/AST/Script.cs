﻿using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST.Directives;
using VooDo.AST.Names;
using VooDo.AST.Statements;
using VooDo.Compilation;
using VooDo.Compilation.Emission;
using VooDo.Runtime;
using VooDo.Utils;

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

        public override Script ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map)
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

        internal override CompilationUnitSyntax EmitNode(Scope _scope, Tagger _tagger)
            => EmitNode(_scope, _tagger, ImmutableArray.Create(new Identifier(Compiler.runtimeReferenceAlias)), null);

        internal CompilationUnitSyntax EmitNode(Scope _scope, Tagger _tagger, ImmutableArray<Identifier> _externAliases, ComplexType? _returnType)
        {
            TypeSyntax? returnType = _returnType?.EmitNode(_scope, _tagger);
            TypeSyntax variableType = SFH.VariableType();
            IEnumerable<ExternAliasDirectiveSyntax> aliases = _externAliases
                .EmptyIfDefault()
                .Select(_i => SF.ExternAliasDirective(_i).Own(_tagger, _i));
            IEnumerable<UsingDirectiveSyntax> usings = Usings.Select(_u => _u.EmitNode(_scope, _tagger));
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
                                    Statements.Select(_s => _s.EmitNode(_scope, _tagger)).ToSeparatedList()));

            ImmutableArray<Scope.GlobalDefinition> globals = _scope.GetGlobalDefinitions();
            VariableDeclarationSyntax EmitGlobalDeclaration(Scope.GlobalDefinition _definition)
            {
                TypeSyntax? type = _definition.Prototype.Global.Type.IsVar
                    ? null
                    : _definition.Prototype.Global.Type.EmitNode(_scope, _tagger);
                return SF.VariableDeclaration(
                            SFH.VariableType(type),
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
                                    ? _definition.Prototype.Global.Initializer!.EmitNode(new Scope(), _tagger)
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
                .Own(_tagger, this);
        }

        public override IEnumerable<Node> Children => ((IEnumerable<Node>) Usings).Concat(Statements);
        public override string ToString() => (string.Join('\n', Usings) + "\n\n" + string.Join('\n', Statements)).Trim();

        #endregion

    }

}
