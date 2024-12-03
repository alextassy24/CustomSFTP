using CustomSftpTool.Interfaces;
using CustomSftpTool.Models;

namespace CustomSftpTool.Services
{
    public class ProfileValidator(ILoggerService logger) : IProfileValidator
    {
        private readonly ILoggerService _logger = logger;

        public bool Validate(ProfileData? profile, string profileName)
        {
            if (profile == null || string.IsNullOrEmpty(profile.Name))
            {
                _logger.LogError($"Error: Profile '{profileName}' not found or is invalid.");
                return false;
            }
            return true;
        }
    }
}
