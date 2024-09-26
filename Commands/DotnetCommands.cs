using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Serilog;

namespace CustomSftpTool.Commands
{
    public static class DotnetCommands
    {
        public static async Task<bool> PublishApplicationAsync(string csprojPath, string outputPath)
        {
            var result = await RunProcessAsync(
                "dotnet",
                $"publish \"{csprojPath}\" -c Release -o \"{outputPath}\""
            );

            if (!result.Success)
            {
                Log.Error($"Error publishing application: {result.Error}");
                return false;
            }

            return true;
        }

        public static async Task<bool> CleanApplicationAsync(string csprojPath)
        {
            var result = await RunProcessAsync("dotnet", $"clean \"{csprojPath}\"");

            if (!result.Success)
            {
                Log.Error($"Error cleaning application: {result.Error}");
                return false;
            }

            return true;
        }

        private static Task<(bool Success, string Error)> RunProcessAsync(
            string fileName,
            string arguments
        )
        {
            var tcs = new TaskCompletionSource<(bool, string)>();

            var processStartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = new Process { StartInfo = processStartInfo, EnableRaisingEvents = true };

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
                var success = process.ExitCode == 0;
                process.Dispose();
                tcs.SetResult((success, error));
            };

            process.Start();
            process.BeginErrorReadLine();

            return tcs.Task;
        }
    }
}
