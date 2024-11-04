using System.Diagnostics;
using System.Text;
using CustomSftpTool.Interfaces;
using CustomSftpTool.Logging;

namespace CustomSftpTool.Services
{
    public class DotnetService : IDotnetService
    {
        public async Task<bool> CleanApplicationAsync(string csprojPath)
        {
            return await RunProcessAsync("dotnet", $"clean \"{csprojPath}\" -v quiet");
        }

        public async Task<bool> PublishApplicationAsync(string csprojPath, string outputPath)
        {
            return await RunProcessAsync(
                "dotnet",
                $"publish \"{csprojPath}\" -c Release -o \"{outputPath}\" -v quiet"
            );
        }

        private static async Task<bool> RunProcessAsync(string fileName, string arguments)
        {
            TaskCompletionSource<bool> tcs = new();
            StringBuilder outputLog = new();
            StringBuilder errorLog = new();
            ILoggerService loggerService = new LoggerService();

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

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    outputLog.AppendLine(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorLog.AppendLine(e.Data);
                }
            };

            process.Exited += (sender, args) =>
            {
                if (outputLog.Length > 0)
                {
                    loggerService.LogInfo($"Process output: {outputLog}");
                }

                if (errorLog.Length > 0)
                {
                    loggerService.LogError($"Process errors: {errorLog}");
                }

                if (process.ExitCode != 0)
                {
                    loggerService.LogError($"Process failed with exit code: {process.ExitCode}");
                }

                tcs.SetResult(process.ExitCode == 0);
                process.Dispose();
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await tcs.Task;
            return tcs.Task.Result;
        }
    }
}
