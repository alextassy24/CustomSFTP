using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
                    var filesToUpload = GetFilesToUpload(
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

                    using (
                        var progressBar = new ProgressBar(
                            totalFiles,
                            "Uploading files...",
                            new ProgressBarOptions
                            {
                                ForegroundColor = ConsoleColor.Green,
                                ForegroundColorDone = ConsoleColor.DarkGreen,
                                BackgroundColor = ConsoleColor.DarkGray,
                                ProgressCharacter = '─'
                            }
                        )
                    )
                    {
                        foreach (var filePair in filesToUpload)
                        {
                            string localFile = filePair.Key;
                            string remoteFile = filePair.Value;

                            using var fileStream = File.OpenRead(localFile);
                            sftpClient.UploadFile(fileStream, remoteFile, true);
                            progressBar.Tick($"Uploaded: {Path.GetFileName(localFile)}");
                        }
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
            foreach (var exclusion in exclusions)
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

            var localFileInfo = new FileInfo(localFilePath);
            var remoteFileInfo = sftp.GetAttributes(remoteFilePath);

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
            var filesToUpload = new List<KeyValuePair<string, string>>();

            var files = Directory.GetFiles(localPath);
            foreach (var file in files)
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

            var directories = Directory.GetDirectories(localPath);
            foreach (var directory in directories)
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

                var subFiles = GetFilesToUpload(sftpClient, directory, remoteDir, exclusions);
                filesToUpload.AddRange(subFiles);
            }

            return filesToUpload;
        }
    }
}
