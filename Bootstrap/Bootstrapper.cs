using CustomSftpTool.Interfaces;
using CustomSftpTool.Logging;
using CustomSftpTool.Parsers;
using CustomSftpTool.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CustomSftpTool.Bootstrap;

public static class Bootstrapper
{
    public static ServiceProvider Initialize()
    {
        var services = new ServiceCollection();

        // Register services
        services.AddSingleton<ILoggerService, LoggerService>();
        services.AddSingleton<IProfileManager, ProfileManager>();
        services.AddSingleton<IDotnetService, DotnetService>();
        services.AddSingleton<IDeployService, DeployService>();
        services.AddSingleton<IBackupService, BackupService>();
        services.AddSingleton<IProfileService, ProfileService>();
        services.AddSingleton<IProfileValidator, ProfileValidator>();
        services.AddSingleton<IProfilePromptService, ProfilePromptService>();
        services.AddSingleton<ISshServiceFactory, SshServiceFactory>();
        services.AddSingleton<ISftpServiceFactory, SftpServiceFactory>();

        // Register parsers and executors
        services.AddSingleton<ICommandParser, CommandParser>();
        services.AddSingleton<ICommandExecutor, CommandExecutor>();

        return services.BuildServiceProvider();
    }
}