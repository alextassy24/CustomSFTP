using CustomSftpTool.Data;
using CustomSftpTool.Interfaces;

namespace CustomSftpTool.Commands.Implementations
{
    public class SystemctlServiceCommand(
        string profileName,
        string action,
        ISshService sshService,
        IProfileService profileService
    ) : ICommand
    {
        public Task Execute()
        {
            // Load profile
            var profile = profileService.LoadProfile(profileName);
            if (profile == null || string.IsNullOrEmpty(profile.ServiceName))
            {
                Message.Display("Invalid or missing profile information.", MessageType.Error);
                return Task.CompletedTask;
            }

            // Execute systemctl command
            sshService.Connect();
            try
            {
                var command = BuildSystemctlCommand(profile.ServiceName);
                var result = sshService.ExecuteCommand(command);

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
                sshService.Disconnect();
            }

            return Task.CompletedTask;
        }

        private string BuildSystemctlCommand(string serviceName)
        {
            return $"sudo systemctl {action} {serviceName}";
        }
    }
}
