using System.Collections.Generic;
using System.IO;

using VooDo.Utils;

namespace VooDo.WinUI.Xaml
{

    internal static class CodeLoader
    {


        private static readonly Dictionary<string, string> s_codeCache = new Dictionary<string, string>();

        internal static string GetCode(string _path)
        {
            _path = NormalizeFilePath.Normalize(_path);
            if (!s_codeCache.TryGetValue(_path, out string? code))
            {
                s_codeCache[_path] = code = File.ReadAllText(_path);
            }
            return code;
        }

    }

}
