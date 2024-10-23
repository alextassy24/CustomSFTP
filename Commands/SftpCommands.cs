using Renci.SshNet;
using Serilog;
using ShellProgressBar;

namespace CustomSftpTool.Commands
{
    public static class SftpCommands
    {
        public static Task<bool> UploadDirectoryAsync(
            SftpClient sftpClient,
            string localPath,
            string remotePath,
            List<string> exclusions,
            bool force = false
        )
        {
            return Task.Run(() =>
            {
                try
                {
                    List<KeyValuePair<string, string>> filesToUpload = GetFilesToUpload(
                        sftpClient,
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

                    using ProgressBar progressBar =
                        new(
                            filesToUpload.Count,
                            "Uploading files...",
                            new ProgressBarOptions
                            {
                                ForegroundColor = ConsoleColor.Green,
                                ForegroundColorDone = ConsoleColor.DarkGreen,
                                BackgroundColor = ConsoleColor.DarkGray,
                                ProgressCharacter = '─'
                            }
                        );

                    foreach (KeyValuePair<string, string> filePair in filesToUpload)
                    {
                        string localFile = filePair.Key;
                        string remoteFile = filePair.Value;

                        using FileStream fileStream = File.OpenRead(localFile);
                        sftpClient.UploadFile(fileStream, remoteFile, true);
                        progressBar.Tick($"Uploaded: {Path.GetFileName(localFile)}");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error uploading files.");
                    return false;
                }

                return true;
            });
        }

        private static bool ShouldExclude(string relativePath, List<string> exclusions)
        {
            relativePath = relativePath.Replace("\\", "/").TrimEnd('/');
            foreach (string exclusion in exclusions)
            {
                string normalizedExclusion = exclusion.Replace("\\", "/").TrimEnd('/');
                if (
                    relativePath.Equals(normalizedExclusion, StringComparison.OrdinalIgnoreCase)
                    || relativePath.StartsWith(
                        normalizedExclusion + "/",
                        StringComparison.OrdinalIgnoreCase
                    )
                    || relativePath.Contains(
                        normalizedExclusion,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return true;
                }
            }
            return false;
        }

        private static bool NeedsUpload(
            SftpClient sftp,
            string localFilePath,
            string remoteFilePath,
            bool force = false
        )
        {
            if (force)
            {
                return true;
            }

            if (!sftp.Exists(remoteFilePath))
            {
                return true; // Remote file does not exist
            }

            FileInfo localFileInfo = new(localFilePath);
            Renci.SshNet.Sftp.SftpFileAttributes remoteFileInfo = sftp.GetAttributes(
                remoteFilePath
            );

            // Compare file sizes and modification times
            if (localFileInfo.Length != remoteFileInfo.Size)
            {
                return true;
            }

            return localFileInfo.LastWriteTimeUtc > remoteFileInfo.LastWriteTime;
        }

        private static List<KeyValuePair<string, string>> GetFilesToUpload(
            SftpClient sftpClient,
            string localPath,
            string remotePath,
            List<string> exclusions,
            bool force = false
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

                string remoteFileName = Path.Combine(remotePath, relativePath).Replace("\\", "/");

                if (NeedsUpload(sftpClient, file, remoteFileName, force))
                {
                    filesToUpload.Add(new KeyValuePair<string, string>(file, remoteFileName));
                }
            }

            return filesToUpload;
        }
    }
}
