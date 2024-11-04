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
        private readonly string _profileName = profileName;
        private ISshService _sshService = sshService;
        private ISftpService _sftpService = sftpService;
        private ILoggerService _logger = logger;
        private readonly IProfileManager _profileManager = profileManager;

        public async Task Execute()
        {
            var profile = _profileManager.LoadProfile(_profileName);
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
                Console.WriteLine($"Invalid or missing profile information for '{_profileName}'.");
                return;
            }
            _sftpService.Connect();

            try
            {
                if (
                    string.IsNullOrEmpty(profileName)
                    || profile == null
                    || string.IsNullOrEmpty(profile.Host)
                    || string.IsNullOrEmpty(profile.UserName)
                    || string.IsNullOrEmpty(profile.RemoteDir)
                    || string.IsNullOrEmpty(profile.ServiceName)
                )
                {
                    _logger.LogError("Missing profile data or required fields are null or empty.");
                    return;
                }

                using SshClient sshClient = SshCommands.CreateSshClient(
                    profile.Host,
                    profile.UserName,
                    profile.Password,
                    profile.PrivateKeyPath
                );

                try
                {
                    sshClient.Connect();
                    if (!sshClient.IsConnected)
                    {
                        Message.Display("Error: Unable to connect to SSH.", MessageType.Error);
                        return;
                    }

                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                    string backupPath = $"/tmp/{profile.ServiceName}-Backup";
                    string tarFilePath = $"{backupPath}/backup-{timestamp}.tar.gz";

                    // Use sshClient.RunCommand to avoid logging output
                    sshClient.RunCommand($"mkdir -p {backupPath}");

                    // Get total number of files to backup
                    SshCommand totalFilesCommand = sshClient.RunCommand(
                        $"find {profile.RemoteDir} -type f | wc -l"
                    );

                    int totalFiles = int.Parse(totalFilesCommand.Result.Trim());
                    if (totalFiles == 0)
                    {
                        Message.Display("No files to backup.", MessageType.Warning);
                        return;
                    }

                    using ProgressBar progressBar =
                        new(
                            totalFiles,
                            "Creating backup...",
                            new ProgressBarOptions
                            {
                                ForegroundColor = ConsoleColor.Cyan,
                                ForegroundColorDone = ConsoleColor.DarkCyan,
                                BackgroundColor = ConsoleColor.DarkGray,
                                ProgressCharacter = '█'
                            }
                        );

                    // Execute tar command with verbose option and read output to update progress bar
                    using SshCommand command = sshClient.CreateCommand(
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
                    if (sshClient.IsConnected)
                    {
                        sshClient.Disconnect();
                    }
                }
            }
            catch (Exception ex)
            {
                Message.Display($"Unexpected error: {ex.Message}", MessageType.Error);
            }

            _sftpService.Disconnect();

            Console.WriteLine($"Backup for service '{profile.ServiceName}' completed.");
        }
    }
}
