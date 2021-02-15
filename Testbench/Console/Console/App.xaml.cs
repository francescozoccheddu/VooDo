using Microsoft.UI.Xaml;
using Microsoft.Win32.SafeHandles;

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace VooDo.ConsoleTestbench
{

    internal sealed partial class App : Application
    {

        [DllImport("kernel32.dll",
         EntryPoint = "GetStdHandle",
         SetLastError = true,
         CharSet = CharSet.Auto,
         CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetStdHandle(int _nStdHandle);
        [DllImport("kernel32.dll",
            EntryPoint = "AllocConsole",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern int AllocConsole();

        private const int c_stdOutputHandle = -11;
        private const int c_codePage = 437;

        public App()
        {
        }

        protected override void OnLaunched(LaunchActivatedEventArgs _args)
        {
            _ = AllocConsole();
            IntPtr stdHandle = GetStdHandle(c_stdOutputHandle);
            SafeFileHandle safeFileHandle = new SafeFileHandle(stdHandle, true);
            FileStream fileStream = new FileStream(safeFileHandle, FileAccess.Write);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding encoding = Encoding.GetEncoding(c_codePage);
            StreamWriter standardOutput = new StreamWriter(fileStream, encoding)
            {
                AutoFlush = true
            };
            Console.SetOut(standardOutput);
            Console.Title = "VooDo Console Testbench";
            Console.CursorSize = 6;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            EntryPoint.Run();
            stopwatch.Stop();
            Console.WriteLine($"End of program after {stopwatch.Elapsed.TotalSeconds:F3}s.");
            _ = Console.ReadKey();
            Exit();
        }

    }

}
