using CustomSftpTool.Data;
using CustomSftpTool.Interfaces;

namespace CustomSftpTool.Commands.Implementations;

public class EditProfileCommand(
    string profileName,
    List<string> fields,
    Dictionary<string, string> fieldSets,
    IProfileService profileService
) : ICommand
{
    public Task Execute()
    {
        var profile = profileService.LoadProfile(profileName);
        if (profile == null)
        {
            Message.Display($"Error: Profile '{profileName}' could not be found.", MessageType.Error);
            return Task.CompletedTask;
        }

        if (fieldSets.Count > 0)
        {
            profileService.UpdateProfileFields(profile, fieldSets);
        }
        else if (fields.Count > 0)
        {
            profileService.PromptToUpdateProfile(profile);
        }

        profileService.RenameProfileFile(profileName, profile.Name!);
        profileService.SaveProfile(profile);

        Message.Display($"Profile '{profile.Name}' updated successfully.", MessageType.Success);
        return Task.CompletedTask;
    }
}