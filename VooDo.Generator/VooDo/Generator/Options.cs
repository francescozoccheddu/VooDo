using Microsoft.CodeAnalysis;

namespace VooDo.Generator
{

    internal static class Options
    {

        private const string c_projectOptionPrefix = "build_property.";
        private const string c_fileOptionPrefix = "build_metadata.AdditionalFiles.";

        internal static string Get(string _name, GeneratorExecutionContext _context, AdditionalText _file)
            => _context.AnalyzerConfigOptions.GetOptions(_file).TryGetValue(c_fileOptionPrefix + _name, out string? option) ? option! : "";

        internal static string Get(string _name, GeneratorExecutionContext _context)
            => _context.AnalyzerConfigOptions.GlobalOptions.TryGetValue(c_projectOptionPrefix + _name, out string? option) ? option! : "";

    }

}
