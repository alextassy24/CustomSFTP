using CustomSftpTool.Data;
using CustomSftpTool.Interfaces;

namespace CustomSftpTool.Commands.Implementations;

public class ListProfilesCommand(IProfileService profileService) : ICommand
{
    public Task Execute()
    {
        var profiles = profileService.GetAllProfiles();
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