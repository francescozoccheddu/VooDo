using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Diagnostics;

using VooDo.Hooks;
using VooDo.Transformation;

namespace VooDo.Utils.Testing
{

    public sealed class TestHook : IHook
    {

        private readonly string m_humanReadableId;

        public TestHook(string _humanReadableId)
            => m_humanReadableId = _humanReadableId;

        public IHookListener Listener { get; set; }

        public void Subscribe(object _object) => Debug.WriteLine($"{nameof(TestHook)}({m_humanReadableId}).{nameof(Subscribe)}({_object})");

        public void Unsubscribe() => Debug.WriteLine($"{nameof(TestHook)}({m_humanReadableId}).{nameof(Unsubscribe)}()");

    }

    public sealed class TestHookInitializerProvider : IHookInitializerProvider
    {

        public IHookInitializer GetHookInitializer(MemberAccessExpressionSyntax _syntax, SemanticModel _semantics) => new TestHookInitializer(_syntax.NormalizeWhitespace().ToFullString());
        public IHookInitializer GetHookInitializer(ElementAccessExpressionSyntax _syntax, SemanticModel _semantics) => new TestHookInitializer(_syntax.NormalizeWhitespace().ToFullString());

    }

    public sealed class TestHookInitializer : IHookInitializer
    {

        private readonly string m_humanReadableId;

        public TestHookInitializer(string _humanReadableId) => m_humanReadableId = _humanReadableId;

        public ExpressionSyntax GetHookInitializerSyntax()
            => SyntaxFactoryHelper.NewObject(typeof(TestHook), m_humanReadableId);

    }

}
