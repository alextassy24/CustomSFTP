using CustomSftpTool.Interfaces;
using CustomSftpTool.Logging;
using CustomSftpTool.Parsers;
using CustomSftpTool.Services;

namespace CustomSftpTool
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            ILoggerService logger = new LoggerService();

            try
            {
                // Initialize basic services
                ProfileManager profileManager = new();
                DotnetService dotnetService = new();

                if (args.Length == 0)
                {
                    ShowHelp();
                    return;
                }

                // Parse command and options
                string commandName = args[0].ToLower();
                var options = args.Skip(1).ToList();

                // Handle commands that don’t require a profile
                if (commandName == "add-profile" || commandName == "list-profiles" || commandName == "help")
                {
                    var commandParser = new CommandParser(profileManager, deployService: null, logger, sshService: null, sftpService: null);
                    var command = commandParser.Parse(args);
                    if (command != null)
                    {
                        await command.Execute();
                    }
                    return;
                }

                // Handle commands that require profile information
                string profileName = options.FirstOrDefault() ?? string.Empty; // Ensure non-null value
                if (string.IsNullOrEmpty(profileName))
                {
                    logger.LogWarning("Profile name is null or empty.");
                    return;
                }

                var profile = profileManager.LoadProfile(profileName);
                if (
                    profile == null
                    || string.IsNullOrEmpty(profile.Host)
                    || string.IsNullOrEmpty(profile.UserName)
                    || (
                        string.IsNullOrEmpty(profile.Password)
                        && string.IsNullOrEmpty(profile.PrivateKeyPath)
                    )
                )
                {
                    logger.LogWarning($"Profile '{profileName}' not found or missing important data.");
                    return;
                }

                // Create SSH and SFTP services based on the loaded profile
                var sftpServiceFactory = new SftpServiceFactory(
                    profile.Host,
                    profile.UserName,
                    profile.Password ?? string.Empty,
                    profile.PrivateKeyPath ?? string.Empty
                );
                var sftpService = sftpServiceFactory.Create();

                var sshServiceFactory = new SshServiceFactory(
                    profile.Host,
                    profile.UserName,
                    profile.Password ?? string.Empty,
                    profile.PrivateKeyPath ?? string.Empty
                );
                var sshService = sshServiceFactory.Create();

                // Initialize the deploy service with profile-based SSH/SFTP services
                IDeployService deployService = new DeployService(
                    dotnetService,
                    sshService,
                    sftpService
                );

                // Parse the command with the full set of services
                var parser = new CommandParser(
                    profileManager,
                    deployService,
                    logger,
                    sshService,
                    sftpService
                );
                var parsedCommand = parser.Parse(args);
                if (parsedCommand != null)
                {
                    await parsedCommand.Execute();
                }
                else
                {
                    logger.LogWarning("Command not recognized.");
                    ShowHelp();
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

        private static void ShowHelp()
        {
            Console.WriteLine("Custom Deployment Tool");
            Console.WriteLine("Usage: customSFTP [command] [options]");
            Console.WriteLine("Commands:");
            Console.WriteLine("  deploy <ProfileName>       - Deploy using a profile");
            Console.WriteLine("  add-profile                - Add a new profile");
            Console.WriteLine("  list-profiles              - List all available profiles");
            Console.WriteLine("  show-profile <ProfileName> - Show details of a profile");
            Console.WriteLine("  edit-profile <ProfileName> - Edit an existing profile");
            Console.WriteLine("  remove-profile <ProfileName> - Remove a profile");
            Console.WriteLine("  help                       - Show help information");
        }
    }
}
