using CustomSftpTool.Data;
using CustomSftpTool.Interfaces;
using CustomSftpTool.Profile;
using Renci.SshNet;
using ShellProgressBar;

namespace CustomSftpTool.Commands.Implementations
{
    public class BackupCommand(
        ISshService sshService,
        ISftpService sftpService,
        IProfileManager profileManager,
        ILoggerService logger,
        string profileName
    ) : ICommand
    {
        private readonly ISshService _sshService = sshService;
        private readonly ISftpService _sftpService = sftpService;
        private readonly ILoggerService _logger = logger;
        private readonly IProfileManager _profileManager = profileManager;

        public Task Execute()
        {
            var profile = _profileManager.LoadProfile(profileName);
            if (
                profile == null
                || string.IsNullOrEmpty(profile.LocalDir)
                || string.IsNullOrEmpty(profile.RemoteDir)
                || string.IsNullOrEmpty(profile.ServiceName)
                || string.IsNullOrEmpty(profile.Host)
                || string.IsNullOrEmpty(profile.UserName)
                || (
                    string.IsNullOrEmpty(profile.Password)
                    && string.IsNullOrEmpty(profile.PrivateKeyPath)
                )
            )
            {
                Console.WriteLine($"Invalid or missing profile information for '{profileName}'.");
                return Task.CompletedTask;
            }

            _sftpService.Connect();

            try
            {
                using var sshClient = SshCommands.CreateSshClient(
                    profile.Host,
                    profile.UserName,
                    profile.Password,
                    profile.PrivateKeyPath
                );

                sshClient.Connect();

                if (!sshClient.IsConnected)
                {
                    Message.Display("Error: Unable to connect to SSH.", MessageType.Error);
                    return Task.CompletedTask;
                }

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                string backupPath = $"/tmp/{profile.ServiceName}-Backup";
                string tarFilePath = $"{backupPath}/backup-{timestamp}.tar.gz";

                sshClient.RunCommand($"mkdir -p {backupPath}");

                SshCommand totalFilesCommand = sshClient.RunCommand(
                    $"find {profile.RemoteDir} -type f | wc -l"
                );

                int totalFiles = int.Parse(totalFilesCommand.Result.Trim());
                if (totalFiles == 0)
                {
                    Message.Display("No files to backup.", MessageType.Warning);
                    return Task.CompletedTask;
                }

                using var progressBar = new ProgressBar(
                    totalFiles,
                    "Creating backup...",
                    new ProgressBarOptions
                    {
                        ForegroundColor = ConsoleColor.Cyan,
                        ForegroundColorDone = ConsoleColor.DarkCyan,
                        BackgroundColor = ConsoleColor.DarkGray,
                        ProgressCharacter = '█',
                    }
                );

                using var command = sshClient.CreateCommand(
                    $"tar -czvf {tarFilePath} -C / {profile.RemoteDir.TrimStart('/')}"
                );

                command.CommandTimeout = TimeSpan.FromMinutes(10);
                var asyncResult = command.BeginExecute();

                using (var outputReader = new StreamReader(command.OutputStream))
                {
                    string? line;
                    while (!asyncResult.IsCompleted || !outputReader.EndOfStream)
                    {
                        line = outputReader.ReadLine();
                        if (!string.IsNullOrEmpty(line))
                        {
                            progressBar.Tick($"Adding: {Path.GetFileName(line)}");
                        }
                    }
                }

                command.EndExecute(asyncResult);

                profile.LastBackup = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                ProfileCommands.SaveProfile(profile.Name ?? profileName, profile);

                Message.Display("Backup created successfully!", MessageType.Success);
            }
            catch (Exception ex)
            {
                Message.Display($"Error creating backup: {ex.Message}", MessageType.Error);
            }
            finally
            {
                _sftpService.Disconnect();
            }

            return Task.CompletedTask;
        }
    }
}
