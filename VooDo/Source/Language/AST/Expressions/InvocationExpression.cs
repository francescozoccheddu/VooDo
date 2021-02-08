using System;
using System.Collections.Immutable;

using VooDo.Language.AST.Names;
using VooDo.Utils;

namespace VooDo.Language.AST.Expressions
{

    public sealed record InvocationExpression(Expression Source, ImmutableArray<InvocationExpression.Argument> Arguments = default) : Expression
    {

        #region Nested types

        public abstract record Argument : IAST
        {

            public enum EKind
            {
                Value, Ref, Out
            }

            internal Argument() { }

            public abstract EKind Kind { get; }

            protected string m_Prefix => Kind switch
            {
                EKind.Value => "",
                EKind.Ref => "ref ",
                EKind.Out => "out ",
                _ => throw new NotImplementedException(),
            };

        }

        public sealed record ValueArgument(Expression Expression) : Argument
        {
            public override EKind Kind => EKind.Value;
            public override string ToString() => $"{m_Prefix}{Expression}";
        }

        public sealed record AssignableArgument(Argument.EKind AssignableKind, AssignableExpression Expression) : Argument
        {
            public override EKind Kind => AssignableKind;
            public override string ToString() => $"{m_Prefix}{Expression}";
        }

        public sealed record OutDeclarationArgument(ComplexTypeOrVar Type, IdentifierOrDiscard Name) : Argument
        {
            public override EKind Kind => EKind.Out;
            public override string ToString() => $"{m_Prefix}{Type} {Name}";
        }

        #endregion

        #region Members

        private ImmutableArray<Argument> m_arguments;
        public ImmutableArray<Argument> Arguments
        {
            get => m_arguments;
            init => m_arguments = value.EmptyIfDefault();
        }

        #endregion

        #region Overrides

        public override string ToString() => $"{Source}({string.Join(", ", Arguments)})";

        #endregion

    }

}
