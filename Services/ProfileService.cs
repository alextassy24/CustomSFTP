using CustomSftpTool.Commands;
using CustomSftpTool.Data;
using CustomSftpTool.Interfaces;
using CustomSftpTool.Models;

namespace CustomSftpTool.Services
{
    public class ProfileService(IProfileManager profileManager) : IProfileService
    {
        private readonly IProfileManager _profileManager = profileManager;

        public ProfileData? LoadProfile(string profileName)
        {
            return _profileManager.LoadProfile(profileName);
        }

        public string GetProfileName(string[] options)
        {
            if (options.Length > 0)
            {
                return options[0];
            }
            return string.Empty;
        }

        public void SaveProfile(ProfileData profile)
        {
            _profileManager.SaveProfile(profile.Name!, profile);
        }

        public List<string> GetAllProfiles()
        {
            return _profileManager.GetAllProfiles();
        }

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
            foreach (var kvp in fieldSets)
            {
                switch (kvp.Key.ToLower())
                {
                    case "--name":
                        profile.Name = kvp.Value;
                        break;
                    case "--host":
                        profile.Host = kvp.Value;
                        break;
                    case "--user-name":
                        profile.UserName = kvp.Value;
                        break;
                    case "--password":
                        profile.Password = kvp.Value;
                        break;
                    case "--private-key-path":
                        profile.PrivateKeyPath = kvp.Value;
                        break;
                    case "--csproj-path":
                        profile.CsprojPath = kvp.Value;
                        break;
                    case "--local-dir":
                        profile.LocalDir = kvp.Value;
                        break;
                    case "--remote-dir":
                        profile.RemoteDir = kvp.Value;
                        break;
                    case "--service-name":
                        profile.ServiceName = kvp.Value;
                        break;
                }
            }
        }

        public void PromptToUpdateProfile(ProfileData profile)
        {
            profile.Name = Prompt("Profile Name", profile.Name, true);
            profile.Host = Prompt("Host", profile.Host, true);
            profile.UserName = Prompt("Username", profile.UserName, true);
            profile.Password = Prompt("Password", profile.Password, true);
            profile.PrivateKeyPath = Prompt("Private Key Path", profile.PrivateKeyPath, true);
            profile.CsprojPath = Prompt("Csproj Path", profile.CsprojPath, true);
            profile.LocalDir = Prompt("Local Directory", profile.LocalDir, true);
            profile.RemoteDir = Prompt("Remote Directory", profile.RemoteDir, true);
            profile.ServiceName = Prompt("Service Name", profile.ServiceName, true);
        }

        public void RenameProfileFile(string oldName, string newName)
        {
            _profileManager.FileNameChange(oldName, newName);
        }

        private static string Prompt(
            string message,
            string? defaultValue = null,
            bool isRequired = false
        )
        {
            while (true)
            {
                Console.Write($"{message} [{defaultValue ?? string.Empty}]: ");
                string? input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                {
                    if (isRequired && string.IsNullOrEmpty(defaultValue))
                    {
                        Message.Display($"{message} is required.", MessageType.Error);
                        continue;
                    }
                    else if (!string.IsNullOrEmpty(defaultValue))
                    {
                        return defaultValue;
                    }
                }
                return input ?? string.Empty;
            }
        }

        private string GetProfilesDirectory()
        {
            var profilesDir = Path.Combine(AppContext.BaseDirectory, "profiles");
            if (!Directory.Exists(profilesDir))
            {
                Directory.CreateDirectory(profilesDir);
            }
            return profilesDir;
        }
    }
}
