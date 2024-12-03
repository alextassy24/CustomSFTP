using CustomSftpTool.Data;
using CustomSftpTool.Interfaces;
using CustomSftpTool.Models;

namespace CustomSftpTool.Commands.Implementations
{
    public class SystemctlServiceCommand(
        string profileName,
        string action,
        ISshService sshService,
        IProfileService profileService
    ) : ICommand
    {
        private readonly string _profileName = profileName;
        private readonly string _action = action;
        private readonly ISshService _sshService = sshService;
        private readonly IProfileService _profileService = profileService;

        public Task Execute()
        {
            // Load profile
            var profile = _profileService.LoadProfile(_profileName);
            if (profile == null || string.IsNullOrEmpty(profile.ServiceName))
            {
                Message.Display("Invalid or missing profile information.", MessageType.Error);
                return Task.CompletedTask;
            }

            // Execute systemctl command
            _sshService.Connect();
            try
            {
                var command = BuildSystemctlCommand(profile.ServiceName);
                var result = _sshService.ExecuteCommand(command);

                if (!string.IsNullOrEmpty(result))
                {
                    Message.Display($"Command Result:\n{result.Trim()}", MessageType.Info);
                }
                else
                {
                    Message.Display("No output from the command.", MessageType.Warning);
                }
            }
            catch (Exception ex)
            {
                Message.Display($"Error executing command: {ex.Message}", MessageType.Error);
            }
            finally
            {
                _sshService.Disconnect();
            }

            return Task.CompletedTask;
        }

        private string BuildSystemctlCommand(string serviceName)
        {
            return $"sudo systemctl {_action} {serviceName}";
        }
    }
}
