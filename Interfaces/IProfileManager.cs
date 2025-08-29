using CustomSftpTool.Models;

namespace CustomSftpTool.Interfaces;

public interface IProfileManager
{
    void AddProfile(ProfileData profile);
    ProfileData? LoadProfile(string profileName);
    void SaveProfile(string profileName, ProfileData profileData);
    void RemoveProfile(string profileName);
    void FileNameChange(string oldName, string newName);
    List<string> GetAllProfiles();
    string GetProfileName(string[] options);
}