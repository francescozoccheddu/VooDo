using Microsoft.CodeAnalysis;

using System;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST.Directives;

namespace VooDo.WinUI.Generator
{
    internal static class UsingsOption
    {

        private static ImmutableArray<UsingDirective> s_defaultDirectives = ImmutableArray.Create<UsingDirective>(
                new UsingStaticDirective("VooDo.WinUI.Animation.AnimatorFactory"),
                new UsingStaticDirective("VooDo.WinUI.Animation.AnimationUtils"),
                new UsingNamespaceDirective("Microsoft.UI.Xaml"),
                new UsingNamespaceDirective("System.Numerics"),
                new UsingNamespaceDirective("System")
            );

        private static UsingDirective ParseSingle(string _value)
        {
            string[] tokens = _value.Split('=');
            if (tokens.Length == 1)
            {
                string[] nameTokens = tokens[0].Split();
                if (nameTokens.Length > 2 || (nameTokens.Length == 2 && !nameTokens[0].Equals("static", StringComparison.OrdinalIgnoreCase)))
                {
                    throw new FormatException("Name cannot contain whitespace");
                }
                if (nameTokens.Length == 1)
                {
                    return new UsingNamespaceDirective(nameTokens[0]);
                }
                else
                {
                    return new UsingStaticDirective(nameTokens[1]);
                }
            }
            else if (tokens.Length == 2)
            {
                return new UsingNamespaceDirective(tokens[0], tokens[1]);
            }
            else
            {
                throw new FormatException("Multiple '=' symbols");
            }
        }

        internal static bool TryGet(GeneratorExecutionContext _context, out ImmutableArray<UsingDirective> _directives)
        {
            string option = OptionRetriever.Get(Identifiers.usingsOption, _context);
            string[] tokens = option.Split(',');
            int count = string.IsNullOrEmpty(tokens.Last()) ? tokens.Length - 1 : tokens.Length;
            UsingDirective[] directives = new UsingDirective[count];
            for (int i = 0; i < count; i++)
            {
                try
                {
                    directives[i] = ParseSingle(tokens[i]);
                }
                catch (Exception e)
                {
                    _context.ReportDiagnostic(DiagnosticFactory.InvalidUsing(tokens[i], e.Message));
                    return false;
                }
            }
            _directives = s_defaultDirectives.AddRange(directives);
            return true;
        }

    }
}
