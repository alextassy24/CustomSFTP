namespace CustomSftpTool.Interfaces;

public interface IBackupService
{
    Task<bool> RunBackupAsync(string profileName);
}