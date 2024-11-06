// Commands/Implementations/RemoveProfileCommand.cs
using CustomSftpTool.Data;
using CustomSftpTool.Interfaces;

namespace CustomSftpTool.Commands.Implementations
{
    public class RemoveProfileCommand(IProfileManager profileManager, string profileName) : ICommand
    {
        private readonly IProfileManager _profileManager = profileManager;
        private readonly string _profileName = profileName;

        public Task Execute()
        {
            var profile = _profileManager.LoadProfile(_profileName);
            if (profile == null)
            {
                Message.Display($"Error: Profile '{_profileName}' not found.", MessageType.Error);
                return Task.CompletedTask;
            }

            _profileManager.RemoveProfile(_profileName);
            Message.Display($"Profile '{_profileName}' removed successfully.", MessageType.Success);

            return Task.CompletedTask;
        }
    }
}
