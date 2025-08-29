using CustomSftpTool.Bootstrap;
using CustomSftpTool.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CustomSftpTool
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            var serviceProvider = Bootstrapper.Initialize();

            var logger = serviceProvider.GetRequiredService<ILoggerService>();
            try
            {
                var commandParser = serviceProvider.GetRequiredService<ICommandParser>();

                if (args.Length == 0)
                {
                    ShowHelp(logger);
                    return;
                }

                var command = commandParser.Parse(args);
                if (command != null)
                {
                    await command.Execute();
                }
                else
                {
                    logger.LogWarning("Command not recognized.");
                    ShowHelp(logger);
                }
            }
            catch (Exception ex)
            {
                logger.LogFatal("An unhandled exception occurred.", ex);
            }
            finally
            {
                logger.CloseAndFlush();
            }
        }

        private static void ShowHelp(ILoggerService logger)
        {
            logger.LogInfo("Custom Deployment Tool");
            logger.LogInfo("Usage: customSFTP [command] [options]");
            logger.LogInfo("Commands:");
            logger.LogInfo("  help                          - Show help information");
            logger.LogInfo("  add-profile                   - Add a new profile");
            logger.LogInfo("  list-profiles                 - List all available profiles");
            logger.LogInfo("  deploy <ProfileName>          - Deploy using a profile");
            logger.LogInfo("  show-profile <ProfileName>    - Show details of a profile");
            logger.LogInfo("  edit-profile <ProfileName>    - Edit an existing profile");
            logger.LogInfo("  remove-profile <ProfileName>  - Remove a profile");
        }
    }
}
