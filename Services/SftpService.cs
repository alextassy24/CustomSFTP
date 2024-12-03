using System.IO;
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
                if (!_sftpClient.IsConnected)
                {
                    _sftpClient.Connect();
                    Log.Information("Connected to SFTP server.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to connect to SFTP server.");
                throw;
            }
        }

        public void Disconnect()
        {
            if (_sftpClient.IsConnected)
            {
                _sftpClient.Disconnect();
                Log.Information("Disconnected from SFTP server.");
            }
        }

        public async Task UploadFileAsync(string localFilePath, string remoteFilePath)
        {
            using var fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read);
            await Task.Run(() => _sftpClient.UploadFile(fileStream, remoteFilePath, true));
            Log.Information($"Uploaded file '{localFilePath}' to '{remoteFilePath}'.");
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
                if (!_sftpClient.IsConnected)
                {
                    throw new InvalidOperationException("SFTP client is not connected.");
                }

                EnsureRemoteDirectoryExists(remotePath);

                var files = Directory.GetFiles(localPath, "*", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    var relativePath = Path.GetRelativePath(localPath, file).Replace("\\", "/");
                    var remoteFilePath = Path.Combine(remotePath, relativePath).Replace("\\", "/");

                    if (ShouldExclude(relativePath, exclusions))
                    {
                        Log.Information($"Excluding file: {file}");
                        continue;
                    }

                    if (!force && RemoteFileMatches(file, remoteFilePath))
                    {
                        Log.Information($"Skipping upload: {file}");
                        continue;
                    }

                    var remoteFileDir = Path.GetDirectoryName(remoteFilePath)?.Replace("\\", "/");
                    if (!string.IsNullOrEmpty(remoteFileDir))
                    {
                        EnsureRemoteDirectoryExists(remoteFileDir);
                    }

                    Log.Information($"Uploading file: {file} to {remoteFilePath}");
                    using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                    await Task.Run(() => _sftpClient.UploadFile(fileStream, remoteFilePath, true));
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error uploading directory.");
                return false;
            }
        }

        public async Task DownloadFileAsync(string remoteFilePath, string localFilePath)
        {
            using var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write);
            await Task.Run(() => _sftpClient.DownloadFile(remoteFilePath, fileStream));
            Log.Information($"Downloaded file '{remoteFilePath}' to '{localFilePath}'.");
        }

        private void EnsureRemoteDirectoryExists(string remotePath)
        {
            var pathParts = remotePath.Split('/');
            var currentPath = "";

            foreach (var part in pathParts)
            {
                currentPath = string.IsNullOrEmpty(currentPath) ? part : $"{currentPath}/{part}";
                if (!_sftpClient.Exists(currentPath))
                {
                    _sftpClient.CreateDirectory(currentPath);
                }
            }
        }

        private static bool ShouldExclude(string relativePath, List<string> exclusions)
        {
            relativePath = relativePath.Replace("\\", "/");
            return exclusions.Any(exclusion =>
                relativePath.StartsWith(exclusion.Replace("\\", "/"))
            );
        }

        private bool RemoteFileMatches(string localFilePath, string remoteFilePath)
        {
            if (!_sftpClient.Exists(remoteFilePath))
            {
                return false; // File does not exist remotely
            }

            var localFileInfo = new FileInfo(localFilePath);
            var remoteFileInfo = _sftpClient.GetAttributes(remoteFilePath);

            return localFileInfo.Length == remoteFileInfo.Size
                && localFileInfo.LastWriteTimeUtc <= remoteFileInfo.LastWriteTime;
        }
    }
}
