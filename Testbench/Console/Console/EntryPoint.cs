using VooDo.Factory;

namespace VooDo.ConsoleTestbench
{
    internal static class EntryPoint
    {

        public static void Run()
        {
            string code = @"
int x = 3;
global int y = 4;
global var z = 8;
            ";
            ScriptSource.FromScript(code)
                .WithAdditionalReferences(Reference.GetSystemReferences())
                .Compile();
        }

    }
}
