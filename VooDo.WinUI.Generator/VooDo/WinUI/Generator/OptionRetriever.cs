using Microsoft.CodeAnalysis;

namespace VooDo.WinUI.Generator
{

    internal static class OptionRetriever
    {

        private const string c_projectOptionPrefix = "build_property.";
        private const string c_fileOptionPrefix = "build_metadata.AdditionalFiles.";

        internal static string Get(string _name, GeneratorExecutionContext _context, AdditionalText _file)
            => _context.AnalyzerConfigOptions.GetOptions(_file).TryGetValue(c_fileOptionPrefix + _name, out string? option) ? option! : "";

        internal static string Get(string _name, GeneratorExecutionContext _context)
            => _context.AnalyzerConfigOptions.GlobalOptions.TryGetValue(c_projectOptionPrefix + _name, out string? option) ? option! : "";

    }

}
