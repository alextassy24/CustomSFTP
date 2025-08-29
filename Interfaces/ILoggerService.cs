namespace CustomSftpTool.Interfaces;

public interface ILoggerService
{
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message);
    void LogDebug(string message);
    void LogFatal(string message, Exception ex);
    void CloseAndFlush();
}