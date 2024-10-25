using System.Diagnostics;
using CustomSftpTool.Interfaces;

namespace CustomSftpTool.Services
{
    public class DotnetService : IDotnetService
    {
        public async Task<bool> CleanApplicationAsync(string csprojPath)
        {
            return await RunProcessAsync("dotnet", $"clean \"{csprojPath}\"");
        }

        public async Task<bool> PublishApplicationAsync(string csprojPath, string outputPath)
        {
            return await RunProcessAsync(
                "dotnet",
                $"publish \"{csprojPath}\" -c Release -o \"{outputPath}\""
            );
        }

        private static async Task<bool> RunProcessAsync(string fileName, string arguments)
        {
            TaskCompletionSource<bool> tcs = new();

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };
            process.Exited += (sender, args) =>
            {
                tcs.SetResult(process.ExitCode == 0);
                process.Dispose();
            };

            process.Start();
            await tcs.Task;
            return tcs.Task.Result;
        }
    }
}
