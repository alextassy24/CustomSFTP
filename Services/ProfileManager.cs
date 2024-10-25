using System.Text.Json;
using CustomSftpTool.Commands;
using CustomSftpTool.Data;
using CustomSftpTool.Interfaces;
using CustomSftpTool.Models;

namespace CustomSftpTool.Services
{
    public class ProfileManager : IProfileManager
    {
        private readonly string _profilesDir;

        public ProfileManager()
        {
            _profilesDir = GetProfilesDirectory();
        }

        private static string GetProfilesDirectory()
        {
            string profilesDir = Path.Combine(AppContext.BaseDirectory, "profiles");
            Directory.CreateDirectory(profilesDir);
            return profilesDir;
        }

        public void AddProfile(ProfileData profile)
        {
            if (string.IsNullOrEmpty(profile.Name))
            {
                return;
            }
            SaveProfile(profile.Name, profile);
        }

        public ProfileData? LoadProfile(string profileName)
        {
            string profilePath = Path.Combine(_profilesDir, $"{profileName}.json");
            if (!File.Exists(profilePath))
            {
                return null;
            }

            string json = File.ReadAllText(profilePath);
            ProfileData? profileData = JsonSerializer.Deserialize<ProfileData>(json);
            return profileData;
        }

        public void SaveProfile(string profileName, ProfileData profileData)
        {
            string profilePath = Path.Combine(_profilesDir, $"{profileName}.json");
            string json = JsonSerializer.Serialize(
                profileData,
                new JsonSerializerOptions { WriteIndented = true }
            );
            File.WriteAllText(profilePath, json);
        }

        public void RemoveProfile(string profileName)
        {
            string profilePath = Path.Combine(_profilesDir, $"{profileName}.json");
            if (File.Exists(profilePath))
            {
                File.Delete(profilePath);
            }
        }

        public List<string> GetAllProfiles()
        {
            if (!Directory.Exists(_profilesDir))
            {
                return [];
            }

            return Directory
                .EnumerateFiles(_profilesDir, "*.json")
                .Select(static file => Path.GetFileNameWithoutExtension(file))
                .ToList();
        }

        public void FileNameChange(string oldName, string newName)
        {
            if (!string.Equals(newName, oldName, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(
                    $"Do you want to rename the profile from '{oldName}' to '{newName}'? (y/n)"
                );
                var response = Console.ReadLine();
                if (!string.Equals(response, "y", StringComparison.OrdinalIgnoreCase))
                {
                    Message.Display("Profile renaming canceled.", MessageType.Warning);
                    return;
                }

                // Rename the JSON file
                var profilesDir = GetProfilesDirectory();
                var oldProfilePath = Path.Combine(profilesDir, $"{oldName}.json");
                var newProfilePath = Path.Combine(profilesDir, $"{newName}.json");

                try
                {
                    File.Move(oldProfilePath, newProfilePath);
                    Message.Display(
                        $"Profile renamed from '{oldName}' to '{newName}'.",
                        MessageType.Success
                    );
                }
                catch (Exception ex)
                {
                    Message.Display(
                        $"Error renaming profile file: {ex.Message}",
                        MessageType.Error
                    );
                    return;
                }
            }
        }

        public string GetProfileName(string[] options)
        {
            return options.FirstOrDefault(opt => !opt.StartsWith("--")) ?? string.Empty;
        }
    }
}
