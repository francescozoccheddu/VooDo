using System;

namespace VooDo.Utils
{

    public static class NormalizeFilePath
    {

        public static string Normalize(string _path)
            => Uri.UnescapeDataString(new Uri(_path).AbsolutePath);

    }

}
