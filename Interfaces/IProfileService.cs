using CustomSftpTool.Models;

namespace CustomSftpTool.Interfaces
{
    public interface IProfileService
    {
        ProfileData? LoadProfile(string profileName);
        string GetProfileName(string[] options);
        void SaveProfile(ProfileData profile);
        void UpdateProfileFields(ProfileData profile, Dictionary<string, string> fieldSets);
        void PromptToUpdateProfile(ProfileData profile);
        void RenameProfileFile(string oldName, string newName);
        List<string> GetAllProfiles();
        void RemoveProfile(string profileName);
    }
}
