using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;

namespace VooDo.Transformation
{

    public class ScriptTransformer
    {

        private const string c_tempAssemblyName = "ScriptTransformer_VooDo_internal_";

        public struct Options
        {

            public static Options Default { get; }
                = new Options
                {
                    HookInitializerProvider = new HookInitializerList()
                };

            public IHookInitializerProvider HookInitializerProvider { get; set; }

            internal void EnsureValid()
            {
                if (HookInitializerProvider == null)
                {
                    throw new NullReferenceException($"{nameof(HookInitializerProvider)} cannot be null");
                }
            }

        }

        public CompilationUnitSyntax Transform(SemanticModel _semantics, SyntaxNode _root, Options _options)
        {
            if (_semantics == null)
            {
                throw new ArgumentNullException(nameof(_semantics));
            }
            if (_root == null)
            {
                throw new ArgumentNullException(nameof(_root));
            }
            try
            {
                _options.EnsureValid();
            }
            catch (Exception exception)
            {
                throw new ArgumentException(nameof(_options), exception);
            }
            throw new NotImplementedException();
        }

        public CompilationUnitSyntax Transform(SemanticModel _semantics, Options _options)
        {
            if (_semantics == null)
            {
                throw new ArgumentNullException(nameof(_semantics));
            }
            return Transform(_semantics, _semantics.SyntaxTree.GetRoot(), _options);
        }

        public CompilationUnitSyntax Transform(CompilationUnitSyntax _root, Options _options)
        {
            if (_root == null)
            {
                throw new ArgumentNullException(nameof(_root));
            }
            SyntaxTree tree = CSharpSyntaxTree.Create(_root);
            CSharpCompilation compilation = CSharpCompilation.Create(c_tempAssemblyName, new SyntaxTree[] { tree });
            return Transform(compilation.GetSemanticModel(tree), _options);
        }

    }

}
