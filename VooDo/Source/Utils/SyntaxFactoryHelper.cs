using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace VooDo.Utils
{

    public static class SyntaxFactoryHelper
    {

        private static CodeDomProvider s_codeDomProvider = null;
        private static SyntaxGenerator s_generator = null;

        public static CodeDomProvider CodeDomProvider => s_codeDomProvider == null
            ? s_codeDomProvider = CodeDomProvider.CreateProvider("CSharp")
            : s_codeDomProvider;

        public static SyntaxGenerator Generator => s_generator == null
            ? s_generator = SyntaxGenerator.GetGenerator(new AdhocWorkspace(), LanguageNames.CSharp)
            : s_generator;

        public static TypeSyntax Type(Type _type)
        {
            if (_type == null)
            {
                throw new ArgumentNullException(nameof(_type));
            }
            CodeTypeReference reference = new CodeTypeReference(_type);
            return SyntaxFactory.ParseTypeName(CodeDomProvider.GetTypeOutput(reference));
        }

        public static ObjectCreationExpressionSyntax NewObject(Type _type, params object[] _literalArguments)
            => NewObject(_type, (IEnumerable<object>) _literalArguments);

        public static ObjectCreationExpressionSyntax NewObject(TypeSyntax _type, params object[] _literalArguments)
            => NewObject(_type, (IEnumerable<object>) _literalArguments);

        public static ObjectCreationExpressionSyntax NewObject(Type _type, IEnumerable<object> _literalArguments)
            => NewObject(Type(_type), _literalArguments);

        public static ObjectCreationExpressionSyntax NewObject(TypeSyntax _type, IEnumerable<object> _literalArguments)
        {
            if (_type == null)
            {
                throw new ArgumentNullException(nameof(_type));
            }
            if (_literalArguments == null)
            {
                throw new ArgumentNullException(nameof(_literalArguments));
            }
            return (ObjectCreationExpressionSyntax) Generator.ObjectCreationExpression(_type, _literalArguments.Select(Generator.LiteralExpression));
        }

        public static InvocationExpressionSyntax StaticMethodCall(Type _type, string _methodName, IEnumerable<object> _literalArguments = null, IEnumerable<Type> _genericArguments = null)
            => StaticMethodCall(Type(_type), _methodName, _literalArguments, _genericArguments?.Select(Type));

        public static InvocationExpressionSyntax StaticMethodCall(TypeSyntax _type, string _methodName, IEnumerable<object> _literalArguments = null, IEnumerable<TypeSyntax> _genericArguments = null)
        {
            if (_type == null)
            {
                throw new ArgumentNullException(nameof(_type));
            }
            if (_methodName == null)
            {
                throw new ArgumentNullException(nameof(_methodName));
            }
            SyntaxNode methodName = _genericArguments == null ? Generator.IdentifierName(_methodName) : Generator.GenericName(_methodName, _genericArguments);
            SyntaxNode method = Generator.MemberAccessExpression(_type, methodName);
            IEnumerable<SyntaxNode> arguments = _literalArguments?.Select(Generator.LiteralExpression) ?? Enumerable.Empty<SyntaxNode>();
            return (InvocationExpressionSyntax) Generator.InvocationExpression(method, arguments);
        }


    }

}
