using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VooDo.Utils
{

    public static class Compiler
    {

        public struct Options
        {

            public static Options Default { get; } = new Options
            {
                AssemblyName = "VooDo Generated Assembly"
            };

            public IEnumerable<Assembly> AdditionalAssemblyReferences { get; set; }
            public string AssemblyName { get; set; }

            public void EnsureValid()
            {
                if (AssemblyName == null)
                {
                    throw new NullReferenceException($"{nameof(AssemblyName)} cannot be null");
                }
            }

        }

        public static CSharpCompilation Compile(CSharpSyntaxTree _tree, Options _options)
        {
            if (_tree == null)
            {
                throw new ArgumentNullException(nameof(_tree));
            }
            CSharpCompilationOptions options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            List<Assembly> assemblies = (_options.AdditionalAssemblyReferences ?? Enumerable.Empty<Assembly>()).ToList();
            assemblies.Add(typeof(object).Assembly);
            assemblies.Add(typeof(Compiler).Assembly);
            IEnumerable<PortableExecutableReference> metadata = assemblies.Select(_a => MetadataReference.CreateFromFile(_a.Location));
            return CSharpCompilation.Create(_options.AssemblyName, new SyntaxTree[] { _tree }, metadata, options);
        }

        public static SemanticModel GetSemantics(CSharpSyntaxNode _root, Options _options)
        {
            CSharpSyntaxTree tree = (CSharpSyntaxTree) CSharpSyntaxTree.Create(_root);
            CSharpCompilation compilation = Compile(tree, _options);
            SemanticModel semantics = compilation.GetSemanticModel(tree);
            if (semantics == null)
            {
                throw new Exception("No semantic model has been generated");
            }
            return semantics;
        }

    }

}
