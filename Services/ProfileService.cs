using CustomSftpTool.Commands;
using CustomSftpTool.Data;
using CustomSftpTool.Interfaces;
using CustomSftpTool.Models;

namespace CustomSftpTool.Services;

public class ProfileService(IProfileManager profileManager) : IProfileService
{
    public ProfileData? LoadProfile(string profileName) => profileManager.LoadProfile(profileName);

    public string GetProfileName(string[] options) => 
        options.FirstOrDefault(opt => !opt.StartsWith("--")) ?? string.Empty;

    public void SaveProfile(ProfileData profile) => 
        profileManager.SaveProfile(profile.Name!, profile);

    public List<string> GetAllProfiles() => profileManager.GetAllProfiles();

    public void RemoveProfile(string profileName)
    {
        var profilePath = Path.Combine(GetProfilesDirectory(), $"{profileName}.json");
        if (!File.Exists(profilePath))
        {
            throw new FileNotFoundException($"Profile '{profileName}' not found.");
        }
        File.Delete(profilePath);
    }

    public void UpdateProfileFields(ProfileData profile, Dictionary<string, string> fieldSets)
    {
        foreach (var (key, value) in fieldSets)
        {
            switch (key.ToLower())
            {
                case "--name": profile.Name = value; break;
                case "--host": profile.Host = value; break;
                case "--user-name": profile.UserName = value; break;
                case "--password": profile.Password = value; break;
                case "--private-key-path": profile.PrivateKeyPath = value; break;
                case "--csproj-path": profile.CsprojPath = value; break;
                case "--local-dir": profile.LocalDir = value; break;
                case "--remote-dir": profile.RemoteDir = value; break;
                case "--service-name": profile.ServiceName = value; break;
                default:
                    Message.Display($"Unknown field: {key}", MessageType.Warning);
                    break;
            }
        }
    }

    public void PromptToUpdateProfile(ProfileData profile)
    {
        profile.Name = PromptForValue("Profile Name", profile.Name, true);
        profile.Host = PromptForValue("Host", profile.Host, true);
        profile.UserName = PromptForValue("Username", profile.UserName, true);
        
        // Handle authentication method
        var authMethod = PromptForValue("Authentication method (password/key)", "password", false);
        if (authMethod.Equals("password", StringComparison.OrdinalIgnoreCase))
        {
            profile.Password = PromptForValue("Password", profile.Password, true);
            profile.PrivateKeyPath = null;
        }
        else
        {
            profile.PrivateKeyPath = PromptForValue("Private Key Path", profile.PrivateKeyPath, true);
            profile.Password = null;
        }
        
        profile.CsprojPath = PromptForValue("Csproj Path", profile.CsprojPath, true);
        profile.LocalDir = PromptForValue("Local Directory", profile.LocalDir, true);
        profile.RemoteDir = PromptForValue("Remote Directory", profile.RemoteDir, true);
        profile.ServiceName = PromptForValue("Service Name", profile.ServiceName, true);
    }

    public void RenameProfileFile(string oldName, string newName) => 
        profileManager.FileNameChange(oldName, newName);

    private static string PromptForValue(string fieldName, string? defaultValue = null, bool isRequired = false)
    {
        while (true)
        {
            Console.Write($"{fieldName} [{defaultValue ?? ""}]: ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
            {
                if (isRequired && string.IsNullOrEmpty(defaultValue))
                {
                    Message.Display($"{fieldName} is required.", MessageType.Error);
                    continue;
                }
                return defaultValue ?? string.Empty;
            }
            
            return input;
        }
    }

    private static string GetProfilesDirectory()
    {
        var profilesDir = Path.Combine(AppContext.BaseDirectory, "profiles");
        Directory.CreateDirectory(profilesDir);
        return profilesDir;
    }
}