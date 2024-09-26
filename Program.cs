using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CustomSftpTool.Commands;
using Renci.SshNet;
using Serilog;

namespace CustomSftpTool
{
    class Program
    {
        // Connection info
        private static readonly string Host = "193.230.3.37";
        private static readonly string UserName = "iciadmin";
        private static readonly string PrivateKeyPath = @"C:\Users\admin\.ssh\id_rsa"; // Updated path to OpenSSH key

        // Define your local and remote directories
        private static readonly string CsprojPath = @"C:\Users\admin\Desktop\CdM\CdM\CdM.csproj";
        private static readonly string LocalDir = @"C:\Users\admin\Desktop\CdMDeploy";
        private static readonly string RemoteDir = "/var/www/case";
        private static readonly string ServiceName = "case";

        // Define exclusions relative to the local directory
        private static readonly List<string> Exclusions = new List<string>
        {
            "appsettings.json",
            "appsettings.Development.json",
            "wwwroot\\Files"
        };

        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(
                    outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .WriteTo.File("logs\\deployment.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                Log.Information(ConsoleColors.Cyan("Starting deployment..."));
                var totalStopwatch = Stopwatch.StartNew();
                Log.Information(ConsoleColors.Cyan("Cleaning the application..."));
                var cleanStopwatch = Stopwatch.StartNew();
                var cleanApp = await DotnetCommands.CleanApplicationAsync(CsprojPath);
                cleanStopwatch.Stop();
                if (!cleanApp)
                {
                    Log.Error(ConsoleColors.Red("Failed to clean the application."));
                    return;
                }
                Log.Information(
                    ConsoleColors.Green("Cleaning completed in {Elapsed} seconds."),
                    cleanStopwatch.Elapsed.TotalSeconds
                );

                Log.Information(ConsoleColors.Cyan("Publishing the application..."));
                var publishStopwatch = Stopwatch.StartNew();
                var publishApp = await DotnetCommands.PublishApplicationAsync(CsprojPath, LocalDir);
                publishStopwatch.Stop();
                if (!publishApp)
                {
                    Log.Error(ConsoleColors.Red("Failed to publish the application."));
                    return;
                }
                Log.Information(
                    ConsoleColors.Green("Publishing completed in {Elapsed} seconds."),
                    publishStopwatch.Elapsed.TotalSeconds
                );

                Log.Information(ConsoleColors.Cyan("Connecting to SSH..."));
                using var sshClient = SshCommands.CreateSshClient(Host, UserName, PrivateKeyPath);

                sshClient.Connect();

                Log.Information(ConsoleColors.Cyan("Checking service status..."));
                string? serviceStatus = SshCommands.ExecuteCommand(
                    sshClient,
                    $"sudo systemctl is-active {ServiceName}"
                );

                if (string.IsNullOrEmpty(serviceStatus) || serviceStatus.Trim() != "active")
                {
                    Log.Warning(
                        ConsoleColors.Red(
                            ($"Service {ServiceName} is not active or failed to check status.")
                        )
                    );
                    sshClient.Disconnect();
                    return;
                }

                Log.Information(ConsoleColors.Green($"Service {ServiceName} is active."));
                Log.Information(ConsoleColors.Cyan($"Stopping service {ServiceName}."));

                string? stopServiceResult = SshCommands.ExecuteCommand(
                    sshClient,
                    $"sudo systemctl stop {ServiceName}"
                );

                if (stopServiceResult == null)
                {
                    Log.Error(ConsoleColors.Red($"Failed to stop service {ServiceName}."));
                    sshClient.Disconnect();
                    return;
                }

                Log.Information(ConsoleColors.Green($"Service {ServiceName} is stopped."));
                Log.Information(ConsoleColors.Cyan("Transferring newer files..."));

                using var sftpClient = new SftpClient(sshClient.ConnectionInfo);
                sftpClient.Connect();

                bool uploadFiles = await SftpCommands.UploadDirectoryAsync(
                    sftpClient,
                    LocalDir,
                    RemoteDir,
                    Exclusions
                );

                sftpClient.Disconnect();

                if (!uploadFiles)
                {
                    Log.Error(ConsoleColors.Red("Failed to transfer newer files."));
                    Log.Information(ConsoleColors.Cyan($"Restarting service {ServiceName}..."));
                    SshCommands.ExecuteCommand(sshClient, $"sudo systemctl start {ServiceName}");
                    sshClient.Disconnect();
                    return;
                }

                Log.Information(ConsoleColors.Cyan("Transferred newer files."));
                Log.Information(ConsoleColors.Cyan($"Restarting service {ServiceName}..."));
                string? startServiceResult = SshCommands.ExecuteCommand(
                    sshClient,
                    $"sudo systemctl start {ServiceName}"
                );

                if (startServiceResult == null)
                {
                    Log.Error(ConsoleColors.Red($"Failed to start service {ServiceName}."));
                    sshClient.Disconnect();
                    return;
                }

                Log.Information(ConsoleColors.Green($"Service {ServiceName} is started."));
                Log.Information(ConsoleColors.Cyan("Checking its status..."));
                await Task.Delay(2000);

                serviceStatus = SshCommands.ExecuteCommand(
                    sshClient,
                    $"sudo systemctl is-active {ServiceName}"
                );

                if (serviceStatus != null && serviceStatus.Trim() == "active")
                {
                    Log.Information(ConsoleColors.Green($"Service {ServiceName} is active again."));
                    Log.Information(ConsoleColors.Green("Deployment completed successfully!"));
                }
                else
                {
                    Log.Error(ConsoleColors.Red($"Service {ServiceName} failed to start."));
                }
                totalStopwatch.Stop();
                Log.Information(
                    ConsoleColors.Green("Deployment completed in {Elapsed} seconds."),
                    totalStopwatch.Elapsed.TotalSeconds
                );
                sshClient.Disconnect();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred during deployment.");
            }
            finally
            {
                Log.Information(ConsoleColors.Green("Deployment process finished."));
                Log.CloseAndFlush();
            }
        }
    }
}
