using CustomSftpTool.Commands.Implementations;
using CustomSftpTool.Interfaces;

namespace CustomSftpTool.Parsers
{
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
        private readonly IProfileService _profileService = profileService;
        private readonly IDeployService _deployService = deployService;
        private readonly ILoggerService _logger = logger;
        private readonly IProfileValidator _profileValidator = profileValidator;
        private readonly IProfilePromptService _profilePromptService = profilePromptService;
        private readonly IBackupService _backupService = backupService;
        private readonly ISshServiceFactory _sshServiceFactory = sshServiceFactory;

        public ICommand? Parse(string[] args)
        {
            if (args.Length == 0)
            {
                _logger.LogInfo("No command specified. Use 'help' to see available commands.");
                return new HelpCommand();
            }

            string commandName = args[0].ToLower();
            string[] options = args.Skip(1).ToArray();

            // Handle commands requiring a profile name
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
                string profileName = _profileService.GetProfileName(options);
                if (string.IsNullOrEmpty(profileName))
                {
                    _logger.LogError($"Profile name is required for the '{commandName}' command.");
                    return null;
                }

                var profile = _profileService.LoadProfile(profileName);
                if (profile == null)
                {
                    _logger.LogError($"Profile '{profileName}' not found.");
                    return null;
                }

                // Handle commands with a valid profile
                switch (commandName)
                {
                    case "show-profile":
                        return new ShowProfileCommand(_profileService, profileName);

                    case "edit-profile":
                        List<string> fields = ExtractFields(options);
                        Dictionary<string, string> fieldSets = ExtractFieldSets(options);
                        return new EditProfileCommand(
                            profileName,
                            fields,
                            fieldSets,
                            _profileService
                        );

                    case "remove-profile":
                        return new RemoveProfileCommand(_profileService, profileName);

                    case "status-service":
                        var sshService = _sshServiceFactory.CreateSshService(profile);
                        return new SystemctlServiceCommand(
                            profileName,
                            "status",
                            sshService,
                            _profileService
                        );

                    case "check-service":
                        sshService = _sshServiceFactory.CreateSshService(profile);
                        return new SystemctlServiceCommand(
                            profileName,
                            "is-active",
                            sshService,
                            _profileService
                        );

                    case "stop-service":
                        sshService = _sshServiceFactory.CreateSshService(profile);
                        return new SystemctlServiceCommand(
                            profileName,
                            "stop",
                            sshService,
                            _profileService
                        );

                    case "start-service":
                        sshService = _sshServiceFactory.CreateSshService(profile);
                        return new SystemctlServiceCommand(
                            profileName,
                            "start",
                            sshService,
                            _profileService
                        );

                    case "restart-service":
                        sshService = _sshServiceFactory.CreateSshService(profile);
                        return new SystemctlServiceCommand(
                            profileName,
                            "restart",
                            sshService,
                            _profileService
                        );

                    case "deploy":
                        bool force = options.Contains("--force");
                        return new DeployCommand(
                            profileName,
                            force,
                            _profileService,
                            _deployService,
                            _profileValidator,
                            _logger
                        );

                    case "backup":
                        return new BackupCommand(profileName, _backupService);
                }
            }

            // Handle non-profile commands
            switch (commandName)
            {
                case "list-profiles":
                    return new ListProfilesCommand(_profileService);

                case "add-profile":
                    return new AddProfileCommand(_profileService, _profilePromptService, _logger);

                case "help":
                    return new HelpCommand();

                default:
                    _logger.LogError(
                        $"Unknown command: '{commandName}'. Use 'help' to see available commands."
                    );
                    return new HelpCommand();
            }
        }

        private static List<string> ExtractFields(string[] options)
        {
            var fields = new List<string>();
            for (int i = 0; i < options.Length; i++)
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
            for (int i = 0; i < options.Length; i++)
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
}
