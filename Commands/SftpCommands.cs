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
            List<string> exclusions
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
                        exclusions
                    );
                    int totalFiles = filesToUpload.Count;

                    if (totalFiles == 0)
                    {
                        Log.Information("No files to upload.");
                        return true;
                    }

                    using ProgressBar progressBar =
                        new(
                            totalFiles,
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
            foreach (string exclusion in exclusions)
            {
                if (relativePath.StartsWith(exclusion, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool NeedsUpload(
            SftpClient sftp,
            string localFilePath,
            string remoteFilePath
        )
        {
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

            if (localFileInfo.LastWriteTimeUtc > remoteFileInfo.LastWriteTime)
            {
                return true;
            }

            return false;
        }

        private static List<KeyValuePair<string, string>> GetFilesToUpload(
            SftpClient sftpClient,
            string localPath,
            string remotePath,
            List<string> exclusions
        )
        {
            List<KeyValuePair<string, string>> filesToUpload = [];

            string[] files = Directory.GetFiles(localPath);
            foreach (string file in files)
            {
                string relativePath = Path.GetRelativePath(localPath, file);
                if (ShouldExclude(relativePath, exclusions))
                    continue;

                string remoteFileName = Path.Combine(remotePath, Path.GetFileName(file))
                    .Replace("\\", "/");

                if (NeedsUpload(sftpClient, file, remoteFileName))
                {
                    filesToUpload.Add(new KeyValuePair<string, string>(file, remoteFileName));
                }
            }

            string[] directories = Directory.GetDirectories(localPath);
            foreach (string directory in directories)
            {
                string dirName = Path.GetFileName(directory);
                string relativePath = Path.GetRelativePath(localPath, directory);

                if (ShouldExclude(relativePath, exclusions))
                    continue;

                string remoteDir = Path.Combine(remotePath, dirName).Replace("\\", "/");
                if (!sftpClient.Exists(remoteDir))
                {
                    sftpClient.CreateDirectory(remoteDir);
                    Log.Debug($"Created remote directory: {remoteDir}");
                }

                List<KeyValuePair<string, string>> subFiles = GetFilesToUpload(
                    sftpClient,
                    directory,
                    remoteDir,
                    exclusions
                );
                filesToUpload.AddRange(subFiles);
            }

            return filesToUpload;
        }
    }
}
