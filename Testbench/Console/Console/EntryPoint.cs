using VooDo.Factory;

namespace VooDo.ConsoleTestbench
{
    internal static class EntryPoint
    {

        public static void Run()
        {
            string code = @"
global int a = 4;
global var b = 8;
global int?[][,] c = 8;
global culo d = 8;
global (int, culo) e = 8;
global (int f1, culo f2) f = 8;
            ";
            ScriptSource.FromScript(code)
                .WithAdditionalReferences(Reference.GetSystemReferences())
                .Compile();
        }

    }
}
