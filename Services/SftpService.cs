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
        await using var fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read);
        await Task.Run(() => sftpClient.UploadFile(fileStream, remoteFilePath, true));
        Log.Information("Uploaded file '{s}' to '{remoteFilePath1}'.", localFilePath, remoteFilePath);
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
            if (!sftpClient.IsConnected)
            {
                throw new InvalidOperationException("SFTP client is not connected.");
            }

            var files = Directory.GetFiles(localPath, "*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var relativePath = Path.GetRelativePath(localPath, file).Replace("\\", "/");
                var remoteFilePath = Path.Combine(remotePath, relativePath).Replace("\\", "/");

                if (ShouldExclude(relativePath, exclusions))
                {
                    Log.Information("Excluding file: {s}", file);
                    continue;
                }

                if (!force && RemoteFileMatches(file, remoteFilePath))
                {
                    continue;
                }

                Path.GetDirectoryName(remoteFilePath)?.Replace("\\", "/");

                Log.Information("Uploading file: {s} to {remoteFilePath1}", file, remoteFilePath);
                await using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                await Task.Run(() => sftpClient.UploadFile(fileStream, remoteFilePath, true));
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
        await using var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write);
        await Task.Run(() => sftpClient.DownloadFile(remoteFilePath, fileStream));
        Log.Information("Downloaded file '{s}' to '{localFilePath1}'.", remoteFilePath, localFilePath);
    }

    private void EnsureRemoteDirectoryExists(string remotePath)
    {
        var pathParts = remotePath.Split('/');
        var currentPath = "";

        foreach (var part in pathParts)
        {
            currentPath = string.IsNullOrEmpty(currentPath) ? part : $"{currentPath}/{part}";
            if (!sftpClient.Exists(currentPath))
            {
                sftpClient.CreateDirectory(currentPath);
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
        if (!sftpClient.Exists(remoteFilePath))
        {
            return false; // File does not exist remotely
        }

        var localFileInfo = new FileInfo(localFilePath);
        var remoteFileInfo = sftpClient.GetAttributes(remoteFilePath);

        return localFileInfo.Length == remoteFileInfo.Size
               && localFileInfo.LastWriteTimeUtc <= remoteFileInfo.LastWriteTime;
    }
}