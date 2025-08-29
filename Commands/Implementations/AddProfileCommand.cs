using CustomSftpTool.Interfaces;

namespace CustomSftpTool.Commands.Implementations;

public class AddProfileCommand(IProfileService profileService, IProfilePromptService profilePromptService, ILoggerService logger) : ICommand
{
    public Task Execute()
    {
        var profileData = profilePromptService.PromptForProfileData();

        profileService.SaveProfile(profileData);

        logger.LogInfo($"Profile '{profileData.Name}' added successfully.");

        return Task.CompletedTask;
    }
}