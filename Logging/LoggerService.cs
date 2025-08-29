using CustomSftpTool.Interfaces;
using Serilog;

namespace CustomSftpTool.Logging;

public class LoggerService : ILoggerService
{
    public LoggerService()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(
                outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            .WriteTo.File("logs\\app.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }

    public void LogInfo(string message)
    {
        Log.Information(message);
    }

    public void LogWarning(string message)
    {
        Log.Warning(message);
    }

    public void LogError(string message)
    {
        Log.Error(message);
    }

    public void LogDebug(string message)
    {
        Log.Debug(message);
    }

    public void LogFatal(string message, Exception ex)
    {
        var shortStackTrace = string.Join(
            "\n",
            ex.StackTrace?.Split('\n').Take(5) ?? new[] { "No stack trace available" }
        );

        Log.Fatal(
            "{Message}\n{ExceptionMessage}\n{ShortStackTrace}",
            message,
            ex.Message,
            shortStackTrace
        );
    }

    public void CloseAndFlush()
    {
        Log.CloseAndFlush();
    }
}