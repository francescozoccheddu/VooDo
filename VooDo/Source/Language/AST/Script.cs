
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Language.AST.Directives;
using VooDo.Language.AST.Names;
using VooDo.Language.AST.Statements;
using VooDo.Language.Linking;
using VooDo.Runtime;
using VooDo.Utils;

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

        internal override SyntaxNode EmitNode(Scope _scope, Marker _marker)
            => EmitNode(_scope, _marker, ImmutableArray.Create(new Identifier(Linker.runtimeReferenceExternAlias)), null);

        internal SyntaxNode EmitNode(Scope _scope, Marker _marker, ImmutableArray<Identifier> _externAliases, ComplexType? _returnType)
        {
            TypeSyntax? returnType = _returnType?.EmitNode(_scope, _marker);
            IEnumerable<ExternAliasDirectiveSyntax> aliases = _externAliases.EmptyIfDefault().Select(_a => SyntaxFactory.ExternAliasDirective(_a));
            IEnumerable<UsingDirectiveSyntax> usings = Usings.Select(_u => _u.EmitNode(_scope, _marker));
            MethodDeclarationSyntax? runMethod = SyntaxFactory.MethodDeclaration(
                                returnType ??
                                    SyntaxFactory.PredefinedType(
                                        SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                                SyntaxFactory.Identifier(returnType is null
                                    ? nameof(Program.Run)
                                    : nameof(Program<object>.Run)))
                            .WithModifiers(
                                SyntaxFactory.TokenList(new[]{
                                        SyntaxFactory.Token(SyntaxKind.ProtectedKeyword),
                                        SyntaxFactory.Token(SyntaxKind.OverrideKeyword)}))
                            .WithBody(
                                Body.EmitNode(_scope, _marker));
            IEnumerable<FieldDeclarationSyntax> globalDeclarations =
                _scope.GetGlobalDefinitions().Select(_g =>
                    SyntaxFactory.FieldDeclaration(
                        SyntaxFactory.List<AttributeListSyntax>(),
                        SyntaxFactory.TokenList(new[] {
                            SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                            SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)
                        }),
                        SyntaxFactory.VariableDeclaration(
                            _g.Global.Type.IsVar
                            ? SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword))
                            : SyntaxFactoryHelper.VariableType(_g.Global.Type.EmitNode(_scope, _marker)),
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.VariableDeclarator(
                                    _g.Identifier,
                                    null,
                                    _g.Global.HasInitializer
                                        ? SyntaxFactory.EqualsValueClause(
                                            _g.Global.Initializer!.EmitNode(_scope, _marker))
                                        : null)))));
            ClassDeclarationSyntax? classDeclaration =
                SyntaxFactory.ClassDeclaration(Linker.generatedClassName)
                    .WithModifiers(
                        SyntaxFactory.TokenList(
                            new[]{
                                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                                SyntaxFactory.Token(SyntaxKind.SealedKeyword)}))
                    .WithBaseList(
                        SyntaxFactory.BaseList(
                            SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                                SyntaxFactory.SimpleBaseType(
                                    SyntaxFactoryHelper.ProgramType(returnType)))))
                    .WithMembers(globalDeclarations
                        .Cast<MemberDeclarationSyntax>()
                        .Append(runMethod)
                        .ToSyntaxList());
            return SyntaxFactory.CompilationUnit(
                    aliases.ToSyntaxList(),
                    usings.ToSyntaxList(),
                    SyntaxFactory.List<AttributeListSyntax>(),
                    SyntaxFactory.SingletonList<MemberDeclarationSyntax>(classDeclaration))
                .Own(_marker, this);
        }

        public override IEnumerable<Node> Children => ((IEnumerable<Node>) Usings).Append(Body);
        public override string ToString() => Usings.Aggregate("", (_a, _u) => $"{_a}{_u}\n") + Body;

        #endregion

    }

}
