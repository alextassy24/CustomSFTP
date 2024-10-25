using CustomSftpTool.Interfaces;
using Renci.SshNet;
using Serilog;

namespace CustomSftpTool.Services
{
    public class SftpService(SftpClient sftpClient) : ISftpService
    {
        private readonly SftpClient _sftpClient = sftpClient;

        public void Connect()
        {
            try
            {
                _sftpClient.Connect();
                Log.Information("Connected to SFTP server.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to connect to SFTP server.");
                throw;
            }
        }

        public void Disconnect()
        {
            try
            {
                if (_sftpClient.IsConnected)
                {
                    _sftpClient.Disconnect();
                    Log.Information("Disconnected from SFTP server.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to disconnect from SFTP server.");
                throw;
            }
        }

        public async Task UploadFileAsync(string localFilePath, string remoteFilePath)
        {
            try
            {
                using FileStream fileStream = File.OpenRead(localFilePath);
                await Task.Run(() => _sftpClient.UploadFile(fileStream, remoteFilePath, true));
                Log.Information($"Uploaded file: {localFilePath} to {remoteFilePath}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error uploading file: {localFilePath}");
                throw;
            }
        }

        public async Task<bool> UploadDirectoryAsync(
            string localPath,
            string remotePath,
            List<string> exclusions,
            bool force = false
        )
        {
            try
            {
                List<KeyValuePair<string, string>> filesToUpload = GetFilesToUpload(
                    localPath,
                    remotePath,
                    exclusions,
                    force
                );

                if (filesToUpload.Count == 0)
                {
                    Log.Information("No files to upload.");
                    return true;
                }

                foreach (KeyValuePair<string, string> filePair in filesToUpload)
                {
                    await UploadFileAsync(filePair.Key, filePair.Value);
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error uploading directory.");
                return false;
            }
        }

        private List<KeyValuePair<string, string>> GetFilesToUpload(
            string localPath,
            string remotePath,
            List<string> exclusions,
            bool force
        )
        {
            List<KeyValuePair<string, string>> filesToUpload = [];

            string[] files = Directory.GetFiles(localPath, "*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                string relativePath = Path.GetRelativePath(localPath, file).Replace("\\", "/");
                if (ShouldExclude(relativePath, exclusions))
                {
                    continue;
                }

                string remoteFilePath = Path.Combine(remotePath, relativePath).Replace("\\", "/");
                if (NeedsUpload(file, remoteFilePath, force))
                {
                    filesToUpload.Add(new KeyValuePair<string, string>(file, remoteFilePath));
                }
            }

            return filesToUpload;
        }

        private static bool ShouldExclude(string relativePath, List<string> exclusions)
        {
            relativePath = relativePath.Replace("\\", "/").TrimEnd('/');
            foreach (string exclusion in exclusions)
            {
                string normalizedExclusion = exclusion.Replace("\\", "/").TrimEnd('/');
                if (
                    relativePath.StartsWith(normalizedExclusion, StringComparison.OrdinalIgnoreCase)
                )
                {
                    return true;
                }
            }

            return false;
        }

        private bool NeedsUpload(string localFilePath, string remoteFilePath, bool force = false)
        {
            if (force)
            {
                return true;
            }

            if (!_sftpClient.Exists(remoteFilePath))
            {
                return true;
            }

            FileInfo localFileInfo = new(localFilePath);
            Renci.SshNet.Sftp.SftpFileAttributes remoteFileInfo = _sftpClient.GetAttributes(
                remoteFilePath
            );

            return localFileInfo.Length != remoteFileInfo.Size
                || localFileInfo.LastWriteTimeUtc > remoteFileInfo.LastWriteTime;
        }
    }
}
