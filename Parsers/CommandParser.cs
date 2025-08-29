using CustomSftpTool.Commands.Implementations;
using CustomSftpTool.Interfaces;

namespace CustomSftpTool.Parsers;

public class CommandParser(
    IProfileService profileService,
    IDeployService deployService,
    ILoggerService logger,
    IProfileValidator profileValidator,
    IProfilePromptService profilePromptService,
    IBackupService backupService,
    ISshServiceFactory sshServiceFactory
) : ICommandParser
{
    public ICommand? Parse(string[] args)
    {
        if (args.Length == 0)
        {
            logger.LogInfo("No command specified. Use 'help' to see available commands.");
            return new HelpCommand();
        }

        var commandName = args[0].ToLower();
        var options = args.Skip(1).ToArray();

        var profileName = profileService.GetProfileName(options);
        if (
            new[]
            {
                "show-profile",
                "edit-profile",
                "remove-profile",
                "status-service",
                "check-service",
                "stop-service",
                "start-service",
                "restart-service",
                "deploy",
                "backup",
            }.Contains(commandName)
        )
        {
            if (string.IsNullOrEmpty(profileName))
            {
                logger.LogError($"Profile name is required for the '{commandName}' command.");
                return null;
            }

            var profile = profileService.LoadProfile(profileName);
            if (profile == null)
            {
                logger.LogError($"Profile '{profileName}' not found.");
                return null;
            }

            Dictionary<string, string> fieldSets = ExtractFieldSets(options);
            switch (commandName)
            {
                case "show-profile":
                    return new ShowProfileCommand(profileService, profileName);

                case "edit-profile":
                    var fields = ExtractFields(options);
                    return new EditProfileCommand(
                        profileName,
                        fields,
                        fieldSets,
                        profileService
                    );

                case "remove-profile":
                    return new RemoveProfileCommand(profileService, profileName);

                case "status-service":
                    var sshService = sshServiceFactory.CreateSshService(profile);
                    return new SystemctlServiceCommand(
                        profileName,
                        "status",
                        sshService,
                        profileService
                    );

                case "check-service":
                    sshService = sshServiceFactory.CreateSshService(profile);
                    return new SystemctlServiceCommand(
                        profileName,
                        "is-active",
                        sshService,
                        profileService
                    );

                case "stop-service":
                    sshService = sshServiceFactory.CreateSshService(profile);
                    return new SystemctlServiceCommand(
                        profileName,
                        "stop",
                        sshService,
                        profileService
                    );

                case "start-service":
                    sshService = sshServiceFactory.CreateSshService(profile);
                    return new SystemctlServiceCommand(
                        profileName,
                        "start",
                        sshService,
                        profileService
                    );

                case "restart-service":
                    sshService = sshServiceFactory.CreateSshService(profile);
                    return new SystemctlServiceCommand(
                        profileName,
                        "restart",
                        sshService,
                        profileService
                    );

                case "deploy":
                    var force = options.Contains("--force");
                    return new DeployCommand(
                        profileName,
                        force,
                        profileService,
                        deployService,
                        profileValidator,
                        logger
                    );

                case "backup":
                    return new BackupCommand(profileName, backupService);
            }
        }

        // Handle non-profile commands
        switch (commandName)
        {
            case "list-profiles":
                return new ListProfilesCommand(profileService);

            case "add-profile":
                return new AddProfileCommand(profileService, profilePromptService, logger);

            case "help":
                return new HelpCommand();

            default:
                logger.LogError(
                    $"Unknown command: '{commandName}'. Use 'help' to see available commands."
                );
                return new HelpCommand();
        }
    }

    private static List<string> ExtractFields(string[] options)
    {
        var fields = new List<string>();
        for (var i = 0; i < options.Length; i++)
        {
            if (
                options[i].StartsWith("--")
                && (i + 1 >= options.Length || options[i + 1].StartsWith("--"))
            )
            {
                fields.Add(options[i]);
            }
        }
        return fields;
    }

    private static Dictionary<string, string> ExtractFieldSets(string[] options)
    {
        var fieldSets = new Dictionary<string, string>();
        for (var i = 0; i < options.Length; i++)
        {
            if (
                options[i].StartsWith("--")
                && i + 1 < options.Length
                && !options[i + 1].StartsWith("--")
            )
            {
                fieldSets[options[i]] = options[i + 1];
                i++;
            }
        }
        return fieldSets;
    }
}