using CustomSftpTool.Interfaces;

namespace CustomSftpTool.Commands.Implementations
{
    public class BackupCommand(string profileName, IBackupService backupService) : ICommand
    {
        private readonly string _profileName = profileName;
        private readonly IBackupService _backupService = backupService;

        public async Task Execute()
        {
            bool success = await _backupService.RunBackupAsync(_profileName);
            if (!success)
            {
                Console.WriteLine($"Backup failed for profile '{_profileName}'.");
            }
        }
    }
}
