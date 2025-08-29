using CustomSftpTool.Interfaces;
using CustomSftpTool.Models;

namespace CustomSftpTool.Services;

public class ProfileValidator(ILoggerService logger) : IProfileValidator
{
    public bool Validate(ProfileData? profile, string profileName)
    {
        if (profile == null || string.IsNullOrEmpty(profile.Name))
        {
            logger.LogError($"Error: Profile '{profileName}' not found or is invalid.");
            return false;
        }
        return true;
    }
}