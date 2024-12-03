using CustomSftpTool.Interfaces;
using ShellProgressBar;

namespace CustomSftpTool.Services
{
    public class BackupService(
        ISshServiceFactory sshServiceFactory,
        ISftpServiceFactory sftpServiceFactory,
        IProfileManager profileManager,
        ILoggerService logger) : IBackupService
    {
        private readonly ISshServiceFactory _sshServiceFactory = sshServiceFactory;
        private readonly ISftpServiceFactory _sftpServiceFactory = sftpServiceFactory;
        private readonly IProfileManager _profileManager = profileManager;
        private readonly ILoggerService _logger = logger;

        public async Task<bool> RunBackupAsync(string profileName)
        {
            var profile = _profileManager.LoadProfile(profileName);
            if (profile == null || string.IsNullOrEmpty(profile.RemoteDir) || string.IsNullOrEmpty(profile.Name))
            {
                _logger.LogWarning($"Profile '{profileName}' not found.");
                return false;
            }

            var sshService = _sshServiceFactory.CreateSshService(profile);
            var sftpService = _sftpServiceFactory.CreateSftpService(profile);

            sshService.Connect();
            try
            {
                int totalFiles = sshService.CountFilesInDirectory(profile.RemoteDir);
                if (totalFiles == 0)
                {
                    _logger.LogInfo("No files to backup.");
                    return true;
                }

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                string backupPath = $"/tmp/{profile.ServiceName}-Backup";
                string tarFilePath = $"{backupPath}/backup-{timestamp}.tar.gz";

                using var progressBar = new ProgressBar(
                    totalFiles,
                    "Creating tar archive...",
                    new ProgressBarOptions
                    {
                        ForegroundColor = ConsoleColor.Cyan,
                        ForegroundColorDone = ConsoleColor.DarkCyan,
                        BackgroundColor = ConsoleColor.DarkGray,
                        ProgressCharacter = '█',
                    }
                );

                bool tarCreated = sshService.CreateTarArchive(profile.RemoteDir, tarFilePath, progressBar);
                if (!tarCreated)
                {
                    _logger.LogError("Failed to create tar archive.");
                    return false;
                }

                sftpService.Connect();
                await sftpService.DownloadFileAsync(tarFilePath, $"{profile.LocalDir}/backup-{timestamp}.tar.gz");
                sftpService.Disconnect();

                profile.LastBackup = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                _profileManager.SaveProfile(profile.Name, profile);

                _logger.LogInfo("Backup completed successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Backup failed: {ex.Message}");
                return false;
            }
            finally
            {
                sshService.Disconnect();
            }
        }
    }
}
