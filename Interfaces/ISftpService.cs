namespace CustomSftpTool.Interfaces;

public interface ISftpService
{
    void Connect();
    void Disconnect();
    Task<bool> UploadDirectoryAsync(string localPath, string remotePath, List<string> exclusions, bool force = false);
    Task DownloadFileAsync(string remoteFilePath, string localFilePath);
}