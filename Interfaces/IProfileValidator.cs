using CustomSftpTool.Models;

namespace CustomSftpTool.Interfaces;

public interface IProfileValidator
{
    bool Validate(ProfileData? profile, string profileName);
}