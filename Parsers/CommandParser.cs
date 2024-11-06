using CustomSftpTool.Commands;
using CustomSftpTool.Commands.Implementations;
using CustomSftpTool.Interfaces;

namespace CustomSftpTool.Parsers
{
    public class CommandParser(
        IProfileManager profileManager,
        IDeployService deployService,
        ILoggerService logger,
        ISshService sshService,
        ISftpService sftpService
    )
    {
        private readonly IProfileManager _profileManager = profileManager;
        private readonly IDeployService _deployService = deployService;
        private readonly ILoggerService _logger = logger;
        private readonly ISshService _sshService = sshService;
        private readonly ISftpService _sftpService = sftpService;

        public ICommand? Parse(string[] args)
        {
            if (args.Length == 0)
            {
                return new HelpCommand();
            }

            string commandName = args[0].ToLower();
            var options = args.Skip(1).ToArray();
            string profileName = _profileManager.GetProfileName(options);
            options = options.Skip(1).ToArray();

            switch (commandName)
            {
                case "deploy":
                    bool force = options.Contains("--force");
                    return new DeployCommand(profileName, force, _profileManager, _deployService);

                case "add-profile":
                    return new AddProfileCommand(_profileManager);

                case "edit-profile":
                    var fields = ExtractFields(options);
                    var fieldSets = ExtractFieldSets(options);
                    return new EditProfileCommand(profileName, fields, fieldSets, _profileManager);

                case "list-profiles":
                    return new ListProfilesCommand(_profileManager);

                case "remove-profile":
                    return new RemoveProfileCommand(_profileManager, profileName);

                case "show-profile":
                    return new ShowProfileCommand(_profileManager, profileName);

                case "status-service":
                    return new SystemctlServiceCommand(
                        profileName,
                        "status",
                        _sshService,
                        _profileManager
                    );

                case "check-service":
                    return new SystemctlServiceCommand(
                        profileName,
                        "is-active",
                        _sshService,
                        _profileManager
                    );

                case "stop-service":
                    return new SystemctlServiceCommand(
                        profileName,
                        "stop",
                        _sshService,
                        _profileManager
                    );

                case "start-service":
                    return new SystemctlServiceCommand(
                        profileName,
                        "start",
                        _sshService,
                        _profileManager
                    );

                case "restart-service":
                    return new SystemctlServiceCommand(
                        profileName,
                        "restart",
                        _sshService,
                        _profileManager
                    );

                case "backup":
                    return new BackupCommand(
                        _sshService,
                        _sftpService,
                        _profileManager,
                        _logger,
                        profileName
                    );

                default:
                    return new HelpCommand(); // Fallback for unknown commands
            }
        }

        private static List<string> ExtractFields(string[] options)
        {
            List<string> fields = new();
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
            Dictionary<string, string> fieldSets = new();
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
