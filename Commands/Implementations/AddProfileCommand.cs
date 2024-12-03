using CustomSftpTool.Interfaces;

namespace CustomSftpTool.Commands.Implementations
{
    public class AddProfileCommand(IProfileService profileService, IProfilePromptService profilePromptService, ILoggerService logger) : ICommand
    {
        private readonly IProfileService _profileService = profileService;
        private readonly IProfilePromptService _profilePromptService = profilePromptService;
        private readonly ILoggerService _logger = logger;

        public Task Execute()
        {
            var profileData = _profilePromptService.PromptForProfileData();

            _profileService.SaveProfile(profileData);

            _logger.LogInfo($"Profile '{profileData.Name}' added successfully.");

            return Task.CompletedTask;
        }
    }
}
