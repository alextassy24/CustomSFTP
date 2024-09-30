using System.Diagnostics;
using Serilog;

namespace CustomSftpTool.Commands
{
    public static class DotnetCommands
    {
        public static async Task<bool> PublishApplicationAsync(string csprojPath, string outputPath)
        {
            (bool Success, string Error) = await RunProcessAsync(
                "dotnet",
                $"publish \"{csprojPath}\" -c Release -o \"{outputPath}\""
            );

            if (!Success)
            {
                Log.Error($"Error publishing application: {Error}");
                return false;
            }

            return true;
        }

        public static async Task<bool> CleanApplicationAsync(string csprojPath)
        {
            (bool Success, string Error) = await RunProcessAsync(
                "dotnet",
                $"clean \"{csprojPath}\""
            );

            if (!Success)
            {
                Log.Error($"Error cleaning application: {Error}");
                return false;
            }

            return true;
        }

        private static Task<(bool Success, string Error)> RunProcessAsync(
            string fileName,
            string arguments
        )
        {
            TaskCompletionSource<(bool, string)> tcs = new();

            ProcessStartInfo processStartInfo =
                new()
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

            Process process = new() { StartInfo = processStartInfo, EnableRaisingEvents = true };

            string error = string.Empty;

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    error += e.Data + Environment.NewLine;
                }
            };

            process.Exited += (sender, e) =>
            {
                bool success = process.ExitCode == 0;
                process.Dispose();
                tcs.SetResult((success, error));
            };

            process.Start();
            process.BeginErrorReadLine();

            return tcs.Task;
        }
    }
}
