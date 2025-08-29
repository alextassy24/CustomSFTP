using ShellProgressBar;

namespace CustomSftpTool.Interfaces;

public interface ISshService
{
    void Connect();
    void Disconnect();
    string? ExecuteCommand(string command);
    int CountFilesInDirectory(string directoryPath);
    bool CreateTarArchive(string sourceDir, string archivePath, ProgressBar progressBar);
}