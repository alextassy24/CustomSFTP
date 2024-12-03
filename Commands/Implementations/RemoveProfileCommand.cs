using CustomSftpTool.Data;
using CustomSftpTool.Interfaces;

namespace CustomSftpTool.Commands.Implementations
{
    public class RemoveProfileCommand(IProfileService profileService, string profileName) : ICommand
    {
        private readonly IProfileService _profileService = profileService;
        private readonly string _profileName = profileName;

        public Task Execute()
        {
            var profile = _profileService.LoadProfile(_profileName);
            if (profile == null)
            {
                Message.Display($"Error: Profile '{_profileName}' not found.", MessageType.Error);
                return Task.CompletedTask;
            }

            Console.WriteLine($"Are you sure you want to delete the profile '{_profileName}'? (y/n):");
            var confirmation = Console.ReadLine();
            if (!string.Equals(confirmation, "y", StringComparison.OrdinalIgnoreCase))
            {
                Message.Display($"Operation canceled. Profile '{_profileName}' was not removed.", MessageType.Warning);
                return Task.CompletedTask;
            }

            _profileService.RemoveProfile(_profileName);
            Message.Display($"Profile '{_profileName}' removed successfully.", MessageType.Success);

            return Task.CompletedTask;
        }
    }
}
