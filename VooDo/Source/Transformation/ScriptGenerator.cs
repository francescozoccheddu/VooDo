using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Linq;

using VooDo.Hooks;
using VooDo.Utils;

namespace VooDo.Transformation
{

    public class ScriptGenerator
    {

        private const string c_tempAssemblyName = "ScriptGenerator_VooDo_internal_";

        public struct Options
        {

            public static Options Default { get; }
                = new Options
                {
                    HookInitializerProvider = new HookInitializerList(),
                    GlobalType = null,
                    Usings = Enumerable.Empty<UsingDirectiveSyntax>(),
                    ClassName = SyntaxFactory.IdentifierName("GeneratedScript"),
                };

            public IHookInitializerProvider HookInitializerProvider { get; set; }
            public Type GlobalType { get; set; }
            public IEnumerable<UsingDirectiveSyntax> Usings { get; set; }
            public SimpleNameSyntax ClassName { get; set; }

            internal void EnsureValid()
            {
                if (HookInitializerProvider == null)
                {
                    throw new NullReferenceException($"{nameof(HookInitializerProvider)} cannot be null");
                }
                if (Usings == null)
                {
                    throw new NullReferenceException($"{nameof(Usings)} cannot be null");
                }
                if (ClassName == null)
                {
                    throw new NullReferenceException($"{nameof(ClassName)} cannot be null");
                }
            }

        }

        private static CompilationUnitSyntax GenerateSyntax(BlockSyntax _body, Options _options, IEnumerable<IHookInitializer> _hookInitializers)
        {

            TypeSyntax baseType = _options.GlobalType != null
                ? SyntaxFactoryHelper.Type(typeof(Script<>).MakeGenericType(_options.GlobalType))
                : SyntaxFactoryHelper.Type(typeof(Script));

            MethodDeclarationSyntax runMethod =
                SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    SyntaxFactory.Identifier(nameof(Script.Run)))
                .WithModifiers(SyntaxFactory.TokenList(new[]{
                    SyntaxFactory.Token(SyntaxKind.ProtectedKeyword),
                    SyntaxFactory.Token(SyntaxKind.OverrideKeyword)}))
                .WithBody(_body);

            ArgumentSyntax baseArgument =
                SyntaxFactory.Argument(
                    SyntaxFactory.ArrayCreationExpression(
                        SyntaxFactory.ArrayType(SyntaxFactoryHelper.Type(typeof(IHook)))
                        .WithRankSpecifiers(
                            SyntaxFactory.SingletonList(
                                SyntaxFactory.ArrayRankSpecifier(
                                    SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                        SyntaxFactory.OmittedArraySizeExpression())))))
                    .WithInitializer(
                        SyntaxFactory.InitializerExpression(
                            SyntaxKind.ArrayInitializerExpression,
                            SyntaxFactory.SeparatedList(
                                _hookInitializers.Select(_i => _i.GetHookInitializerSyntax())))));

            ConstructorDeclarationSyntax constructor =
                SyntaxFactory.ConstructorDeclaration(_options.ClassName.Identifier)
                    .WithModifiers(SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                    .WithInitializer(
                        SyntaxFactory.ConstructorInitializer(
                            SyntaxKind.BaseConstructorInitializer,
                            SyntaxFactory.ArgumentList(
                                SyntaxFactory.SingletonSeparatedList(baseArgument))))
                    .WithBody(SyntaxFactory.Block());

            ClassDeclarationSyntax classDeclaration =
                SyntaxFactory.ClassDeclaration(_options.ClassName.Identifier)
                    .WithModifiers(SyntaxFactory.TokenList(new[]{
                                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                                SyntaxFactory.Token(SyntaxKind.SealedKeyword)}))
                    .WithBaseList(SyntaxFactory.BaseList(
                            SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                                SyntaxFactory.SimpleBaseType(baseType))))
                    .WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[] { constructor, runMethod }));

            CompilationUnitSyntax unit =
                SyntaxFactory.CompilationUnit()
                .WithUsings(SyntaxFactory.List(_options.Usings))
                .WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(classDeclaration));

            return unit;
        }

        public static CompilationUnitSyntax Generate(BlockSyntax _body, Options _options)
        {
            if (_body == null)
            {
                throw new ArgumentNullException(nameof(_body));
            }
            try
            {
                _options.EnsureValid();
            }
            catch (Exception exception)
            {
                throw new ArgumentException(nameof(_options), exception);
            }
            CompilationUnitSyntax unit = GenerateSyntax(_body, _options, Enumerable.Empty<IHookInitializer>());
            SyntaxTree tree = CSharpSyntaxTree.Create(unit);
            CSharpCompilation compilation = CSharpCompilation.Create(c_tempAssemblyName, new SyntaxTree[] { tree });

            MemberAccessExpressionSyntax method = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.BaseExpression(),
                SyntaxFactory.IdentifierName(nameof(Script.SubscribeHook)));

            ExpressionHookGluer expressionHookGluer = new ExpressionHookGluer(_options.HookInitializerProvider, method);

            BlockSyntax gluedBody = (BlockSyntax) expressionHookGluer.Glue(compilation.GetSemanticModel(tree), _body);

            CompilationUnitSyntax gluedUnit = GenerateSyntax(gluedBody, _options, expressionHookGluer.HookInitializers);

            return gluedUnit;
        }

        public static CompilationUnitSyntax Generate(ArrowExpressionClauseSyntax _body, Options _options)
        {
            if (_body == null)
            {
                throw new ArgumentNullException(nameof(_body));
            }
            return Generate(SyntaxFactory.Block(SyntaxFactory.SingletonList(SyntaxFactory.ReturnStatement(_body.Expression))), _options);
        }
    }

}
