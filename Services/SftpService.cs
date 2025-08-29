using CustomSftpTool.Interfaces;
using Renci.SshNet;
using Serilog;

namespace CustomSftpTool.Services;

public class SftpService(SftpClient sftpClient) : ISftpService
{
    public void Connect()
    {
        try
        {
            if (!sftpClient.IsConnected)
            {
                sftpClient.Connect();
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
        if (sftpClient.IsConnected)
        {
            sftpClient.Disconnect();
            Log.Information("Disconnected from SFTP server.");
        }
    }

    public async Task UploadFileAsync(string localFilePath, string remoteFilePath)
    {
        // Ensure the remote directory exists before uploading
        var remoteDir = Path.GetDirectoryName(remoteFilePath)?.Replace("\\", "/");
        if (!string.IsNullOrEmpty(remoteDir))
        {
            EnsureRemoteDirectoryExists(remoteDir);
        }

        await using var fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read);
        await Task.Run(() => sftpClient.UploadFile(fileStream, remoteFilePath, true));
        Log.Information("Uploaded file '{LocalPath}' to '{RemotePath}'.", localFilePath, remoteFilePath);
    }

    public async Task<bool> UploadDirectoryAsync(
        string localPath,
        string remotePath,
        List<string> exclusions,
        bool force = false)
    {
        try
        {
            if (!sftpClient.IsConnected)
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
                    Log.Information("Excluding file: {FilePath}", file);
                    continue;
                }

                if (!force && RemoteFileMatches(file, remoteFilePath))
                {
                    Log.Debug("Skipping unchanged file: {FilePath}", file);
                    continue;
                }

                var remoteDir = Path.GetDirectoryName(remoteFilePath)?.Replace("\\", "/");
                if (!string.IsNullOrEmpty(remoteDir))
                {
                    EnsureRemoteDirectoryExists(remoteDir);
                }

                Log.Information("Uploading file: {LocalPath} to {RemotePath}", file, remoteFilePath);
                
                try
                {
                    await using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                    await Task.Run(() => sftpClient.UploadFile(fileStream, remoteFilePath, true));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to upload file: {FilePath}", file);
                    throw;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error uploading directory from {LocalPath} to {RemotePath}.", localPath, remotePath);
            return false;
        }
    }

    public async Task DownloadFileAsync(string remoteFilePath, string localFilePath)
    {
        var localDir = Path.GetDirectoryName(localFilePath);
        if (!string.IsNullOrEmpty(localDir))
        {
            Directory.CreateDirectory(localDir);
        }

        await using var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write);
        await Task.Run(() => sftpClient.DownloadFile(remoteFilePath, fileStream));
        Log.Information("Downloaded file '{RemotePath}' to '{LocalPath}'.", remoteFilePath, localFilePath);
    }

    private void EnsureRemoteDirectoryExists(string remotePath)
    {
        try
        {
            remotePath = remotePath.Replace("\\", "/");
            
            var pathParts = remotePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var currentPath = remotePath.StartsWith($"/") ? "/" : "";

            foreach (var part in pathParts)
            {
                currentPath = currentPath == "/" ? $"/{part}" : $"{currentPath}/{part}";
                
                if (!sftpClient.Exists(currentPath))
                {
                    Log.Debug("Creating remote directory: {Directory}", currentPath);
                    sftpClient.CreateDirectory(currentPath);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create remote directory: {Directory}", remotePath);
            throw;
        }
    }

    private static bool ShouldExclude(string relativePath, List<string> exclusions)
    {
        relativePath = relativePath.Replace("\\", "/");
        return exclusions.Any(exclusion =>
            relativePath.StartsWith(exclusion.Replace("\\", "/"), StringComparison.OrdinalIgnoreCase)
        );
    }

    private bool RemoteFileMatches(string localFilePath, string remoteFilePath)
    {
        try
        {
            if (!sftpClient.Exists(remoteFilePath))
            {
                return false;
            }

            var localFileInfo = new FileInfo(localFilePath);
            var remoteFileInfo = sftpClient.GetAttributes(remoteFilePath);

            return localFileInfo.Length == remoteFileInfo.Size &&
                   localFileInfo.LastWriteTimeUtc <= remoteFileInfo.LastWriteTime;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Could not compare file attributes for {FilePath}", remoteFilePath);
            return false;
        }
    }
}