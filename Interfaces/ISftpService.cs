namespace CustomSftpTool.Interfaces
{
    public interface ISftpService
    {
        void Connect();
        void Disconnect();
        Task<string> UploadFileAsync(string localFilePath, string remoteFilePath);
        Task<bool> UploadDirectoryAsync(
            string localPath,
            string remotePath,
            List<string> exclusions,
            bool force = false
        );
    }
}
