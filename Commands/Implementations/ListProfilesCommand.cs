// Commands/Implementations/ListProfilesCommand.cs
using CustomSftpTool.Data;
using CustomSftpTool.Interfaces;

namespace CustomSftpTool.Commands.Implementations
{
    public class ListProfilesCommand(IProfileManager profileManager) : ICommand
    {
        private readonly IProfileManager _profileManager = profileManager;

        public Task Execute()
        {
            var profiles = _profileManager.GetAllProfiles();

            if (profiles.Count == 0)
            {
                Message.Display("No profiles found.", MessageType.Info);
            }
            else
            {
                Message.Display("Available profiles:", MessageType.Info);
                foreach (var profile in profiles)
                {
                    Message.Display($"- {profile}", MessageType.Success);
                }
            }

            return Task.CompletedTask;
        }
    }
}
