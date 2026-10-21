using System;
using System.Diagnostics;
using System.IO;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string applicationDirectory = Path.Combine(baseDirectory, "libs");
        string executablePath = Path.Combine(applicationDirectory, "MathTutor.exe");

        if (!File.Exists(executablePath))
        {
            return;
        }

        ProcessStartInfo startInfo = new ProcessStartInfo(executablePath);
        startInfo.WorkingDirectory = applicationDirectory;
        startInfo.UseShellExecute = true;
        Process.Start(startInfo);
    }
}
