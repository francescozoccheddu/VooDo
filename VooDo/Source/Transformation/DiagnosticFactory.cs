using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using System;

namespace VooDo.Transformation
{

    internal static class DiagnosticFactory
    {

        private static Location GetLocation(SyntaxNodeOrToken _node)
        {
            if (_node == null)
            {
                throw new ArgumentNullException(nameof(_node));
            }
            TextSpan span;
            if (_node.IsNode)
            {
                span = _node.AsNode().GetOriginalOrFullSpan();
            }
            else
            {
                span = _node.AsToken().GetOriginalOrFullSpan();
            }
            return Location.Create(_node.SyntaxTree, span);
        }

        private static int s_id = 0;
        private static string s_Id => $"VD{++s_id:D3}";

        private static readonly DiagnosticDescriptor s_forbiddenSyntax = new DiagnosticDescriptor(
            s_Id,
            "Forbidden syntax",
            "Forbidden {0} syntax encountered",
            "Syntax",
            DiagnosticSeverity.Error,
            true);

        private static readonly DiagnosticDescriptor s_controllerOfNonGlobalVariable = new DiagnosticDescriptor(
            s_Id,
            "Controller of non global variable",
            "Controller of non global variable {0} encountered",
            "Syntax",
            DiagnosticSeverity.Error,
            true);

        private static readonly DiagnosticDescriptor s_controllerOfRefKindArgument = new DiagnosticDescriptor(
            s_Id,
            "Controller of ref-kind argument",
            "Controller of {0} argument encountered",
            "Syntax",
            DiagnosticSeverity.Error,
            true);

        private static readonly DiagnosticDescriptor s_controllerOfZeroOrMultipleArguments = new DiagnosticDescriptor(
            s_Id,
            "Controller of zero or more than one arguments",
            "Controller of {0} arguments encountered",
            "Syntax",
            DiagnosticSeverity.Error,
            true);

        private static readonly DiagnosticDescriptor s_eventCatcherWithoutMemberAccess = new DiagnosticDescriptor(
            s_Id,
            "Event catcher without member access expression",
            "Event catcher without member access expression",
            "Syntax",
            DiagnosticSeverity.Error,
            true);

        private static readonly DiagnosticDescriptor s_eventCatcherEventHandlerErrorType = new DiagnosticDescriptor(
            s_Id,
            "Event catcher with unexpected event handler type",
            "Event catcher with unexpected event handler type {0}",
            "Syntax",
            DiagnosticSeverity.Error,
            true);

        private static readonly DiagnosticDescriptor s_eventCatcherEventHandlerParameterErrorType = new DiagnosticDescriptor(
            s_Id,
            "Event catcher with unexpected event handler parameter type",
            "Event catcher with unexpected event handler parameter {0} type {1}",
            "Syntax",
            DiagnosticSeverity.Error,
            true);

        private static readonly DiagnosticDescriptor s_eventCatcherArgumentTypeMismatch = new DiagnosticDescriptor(
            s_Id,
            "Event catcher argument type mismatch",
            "Event catcher argument {0} type mismatch (expected {1} but provided {2})",
            "Syntax",
            DiagnosticSeverity.Error,
            true);

        private static readonly DiagnosticDescriptor s_eventCatcherArgumentWithMultipleDeclarations = new DiagnosticDescriptor(
            s_Id,
            "Event catcher argument with more than one declarations",
            "Event catcher argument with more than one declarations",
            "Syntax",
            DiagnosticSeverity.Error,
            true);

        private static string CoalesceName(string _name)
            => _name ?? "<unknown-name>";

        private static string CoalesceType(string _type)
            => _type ?? "<unknown-type>";

        internal static Diagnostic ForbiddenSyntax(SyntaxNode _syntax)
            => Diagnostic.Create(s_forbiddenSyntax, GetLocation(_syntax), Enum.GetName(typeof(SyntaxKind), _syntax.Kind()));

        internal static Diagnostic ControllerOfNonGlobalVariable(ExpressionSyntax _variableNode, string _variableName)
            => Diagnostic.Create(s_controllerOfNonGlobalVariable, GetLocation(_variableNode), CoalesceName(_variableName));

        internal static Diagnostic ControllerOfRefKindArgument(SyntaxToken _refKindToken)
            => Diagnostic.Create(s_controllerOfRefKindArgument, GetLocation(_refKindToken), _refKindToken.ValueText);

        internal static Diagnostic ControllerOfZeroOrMultipleArguments(InvocationExpressionSyntax _controllerOfNode)
            => Diagnostic.Create(s_controllerOfZeroOrMultipleArguments, GetLocation(_controllerOfNode), _controllerOfNode.ArgumentList.Arguments.Count);

        internal static Diagnostic EventCatcherWithoutMemberAccess(ExpressionSyntax _eventCatcherNode)
            => Diagnostic.Create(s_eventCatcherWithoutMemberAccess, GetLocation(_eventCatcherNode));

        internal static Diagnostic EventCatcherEventHandlerErrorType(ExpressionSyntax _eventCatcherNode, string _eventHandlerType)
            => Diagnostic.Create(s_eventCatcherEventHandlerErrorType, GetLocation(_eventCatcherNode), CoalesceType(_eventHandlerType));

        internal static Diagnostic EventCatcherEventHandlerParameterErrorType(ArgumentSyntax _relatedArgument, string _parameterName, string _parameterType)
            => Diagnostic.Create(s_eventCatcherEventHandlerParameterErrorType, GetLocation(_relatedArgument), CoalesceName(_parameterName), CoalesceType(_parameterType));

        internal static Diagnostic EventCatcherArgumentTypeMismatch(ArgumentSyntax _argument, string _parameterName, string _parameterType, string _argumentType)
            => Diagnostic.Create(s_eventCatcherArgumentTypeMismatch, GetLocation(_argument), CoalesceName(_parameterName), CoalesceType(_parameterType), CoalesceType(_argumentType));

        internal static Diagnostic EventCatcherArgumentWithMultipleDeclarations(ArgumentSyntax _argument)
            => Diagnostic.Create(s_eventCatcherArgumentWithMultipleDeclarations, GetLocation(_argument));


        internal static TransformationException AsThrowable(this Diagnostic _diagnostic)
            => new TransformationException(_diagnostic);

    }

}
