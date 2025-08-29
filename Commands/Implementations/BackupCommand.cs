using CustomSftpTool.Interfaces;

namespace CustomSftpTool.Commands.Implementations;

public class BackupCommand(string profileName, IBackupService backupService) : ICommand
{
    public async Task Execute()
    {
        var success = await backupService.RunBackupAsync(profileName);
        if (!success)
        {
            Console.WriteLine($"Backup failed for profile '{profileName}'.");
        }
    }
}