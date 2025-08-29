using CustomSftpTool.Data;
using CustomSftpTool.Interfaces;

namespace CustomSftpTool.Commands.Implementations;

public class RemoveProfileCommand(IProfileService profileService, string profileName) : ICommand
{
    public Task Execute()
    {
        var profile = profileService.LoadProfile(profileName);
        if (profile == null)
        {
            Message.Display($"Error: Profile '{profileName}' not found.", MessageType.Error);
            return Task.CompletedTask;
        }

        Console.WriteLine($"Are you sure you want to delete the profile '{profileName}'? (y/n):");
        var confirmation = Console.ReadLine();
        if (!string.Equals(confirmation, "y", StringComparison.OrdinalIgnoreCase))
        {
            Message.Display($"Operation canceled. Profile '{profileName}' was not removed.", MessageType.Warning);
            return Task.CompletedTask;
        }

        profileService.RemoveProfile(profileName);
        Message.Display($"Profile '{profileName}' removed successfully.", MessageType.Success);

        return Task.CompletedTask;
    }
}