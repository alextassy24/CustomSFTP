using CustomSftpTool.Interfaces;
using ShellProgressBar;

namespace CustomSftpTool.Services;

public class BackupService(
    ISshServiceFactory sshServiceFactory,
    ISftpServiceFactory sftpServiceFactory,
    IProfileManager profileManager,
    ILoggerService logger) : IBackupService
{
    public async Task<bool> RunBackupAsync(string profileName)
    {
        var profile = profileManager.LoadProfile(profileName);
        if (profile == null || string.IsNullOrEmpty(profile.RemoteDir) || string.IsNullOrEmpty(profile.Name))
        {
            logger.LogWarning($"Profile '{profileName}' not found.");
            return false;
        }

        var sshService = sshServiceFactory.CreateSshService(profile);
        var sftpService = sftpServiceFactory.CreateSftpService(profile);

        sshService.Connect();
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        var backupPath = $"/tmp/{profile.ServiceName}-Backup";
            
        try
        {
            var totalFiles = sshService.CountFilesInDirectory(profile.RemoteDir);
            if (totalFiles == 0)
            {
                logger.LogInfo("No files to backup.");
                return true;
            }

            var tarFilePath = $"{backupPath}/backup-{timestamp}.tar.gz";

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

            var tarCreated = sshService.CreateTarArchive(profile.RemoteDir, tarFilePath, progressBar);
            if (!tarCreated)
            {
                logger.LogError("Failed to create tar archive.");
                return false;
            }

            sftpService.Connect();
            await sftpService.DownloadFileAsync(tarFilePath, $"{profile.LocalDir}/backup-{timestamp}.tar.gz");
            sftpService.Disconnect();

            profile.LastBackup = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            profileManager.SaveProfile(profile.Name, profile);

            logger.LogInfo("Backup completed successfully.");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError($"Backup failed: {ex.Message}");
            return false;
        }
        finally
        {
            sshService.Disconnect();
        }
    }
}