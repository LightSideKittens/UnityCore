using System.Diagnostics;
using static LSPaths;

public static class PythonRunner
{
    public static void Run(string filePath, string args)
    {
        var start = new ProcessStartInfo();
        start.FileName = @"C:\Python311\python.exe";
        start.Arguments = $"{Python}/{filePath}.py {args}".ToFull();
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        start.CreateNoWindow = true;
        start.WindowStyle = ProcessWindowStyle.Hidden;
        var process = new Process();
        process.StartInfo = start;

        process.Start();
    }
}
