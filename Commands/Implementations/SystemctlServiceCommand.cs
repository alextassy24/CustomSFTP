using CustomSftpTool.Interfaces;

namespace CustomSftpTool.Commands.Implementations
{
    public class SystemctlServiceCommand(
        string profileName,
        string action,
        ISshService sshService,
        IProfileManager profileManager
    ) : ICommand
    {
        private readonly string _profileName = profileName;
        private readonly ISshService _sshService = sshService;
        private readonly IProfileManager _profileManager = profileManager;
        private readonly string _action = action;

        public async Task Execute()
        {
            var profile = _profileManager.LoadProfile(_profileName);
            if (
                profile == null
                || string.IsNullOrEmpty(profile.ServiceName)
                || string.IsNullOrEmpty(_profileName)
            )
            {
                Console.WriteLine("Invalid or missing profile information.");
                return;
            }

            _sshService.Connect();
            string result = _sshService.ExecuteCommand(
                $"sudo systemctl {_action} {profile.ServiceName}"
            );
            Console.WriteLine(result);
            _sshService.Disconnect();
        }
    }
}
