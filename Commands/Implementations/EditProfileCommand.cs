// Commands/Implementations/EditProfileCommand.cs
using CustomSftpTool.Data;
using CustomSftpTool.Interfaces;
using CustomSftpTool.Models;

namespace CustomSftpTool.Commands.Implementations
{
    public class EditProfileCommand(
        string profileName,
        List<string> fields,
        Dictionary<string, string> fieldSets,
        IProfileManager profileManager
    ) : ICommand
    {
        private readonly string _profileName = profileName;
        private readonly List<string> _fields = fields;
        private readonly Dictionary<string, string> _fieldSets = fieldSets;
        private readonly IProfileManager _profileManager = profileManager;

        public Task Execute()
        {
            var existingProfile = _profileManager.LoadProfile(_profileName);
            if (existingProfile == null || string.IsNullOrEmpty(existingProfile.Name))
            {
                Message.Display(
                    $"Error: Profile '{_profileName}' could not be found.",
                    MessageType.Error
                );
                return Task.CompletedTask;
            }

            bool hasFields = _fields != null && _fields.Count > 0;
            bool hasFieldsets = _fieldSets != null && _fieldSets.Count > 0;

            if (hasFieldsets && _fieldSets != null)
            {
                try
                {
                    foreach (var kvp in _fieldSets)
                    {
                        switch (kvp.Key.ToLower())
                        {
                            case "--name":
                                existingProfile.Name = kvp.Value;
                                break;
                            case "--host":
                                existingProfile.Host = kvp.Value;
                                break;
                            case "--user-name":
                                existingProfile.UserName = kvp.Value;
                                break;
                            case "--password":
                                existingProfile.Password = kvp.Value;
                                break;
                            case "--private-key-path":
                                existingProfile.PrivateKeyPath = kvp.Value;
                                break;
                            case "--csproj-path":
                                existingProfile.CsprojPath = kvp.Value;
                                break;
                            case "--local-dir":
                                existingProfile.LocalDir = kvp.Value;
                                break;
                            case "--remote-dir":
                                existingProfile.RemoteDir = kvp.Value;
                                break;
                            case "--service-name":
                                existingProfile.ServiceName = kvp.Value;
                                break;
                        }
                    }
                    _profileManager.FileNameChange(_profileName, existingProfile.Name);
                    _profileManager.SaveProfile(existingProfile.Name, existingProfile);
                    Message.Display(
                        $"Profile '{existingProfile.Name}' updated successfully.",
                        MessageType.Success
                    );
                }
                catch (Exception ex)
                {
                    Message.Display($"Error: {ex.Message}", MessageType.Error);
                    return Task.CompletedTask;
                }
            }
            if (hasFields && _fields != null)
            {
                try
                {
                    foreach (var field in _fields)
                    {
                        switch (field.ToLower())
                        {
                            case "--name":
                                existingProfile.Name = Prompt(
                                    "Profile Name",
                                    existingProfile.Name,
                                    true
                                );
                                break;
                            case "--host":
                                existingProfile.Host = Prompt("Host", existingProfile.Host, true);
                                break;
                            case "--service-name":
                                existingProfile.ServiceName = Prompt(
                                    "Service Name",
                                    existingProfile.ServiceName,
                                    true
                                );
                                break;
                            case "--user-name":
                                existingProfile.UserName = Prompt(
                                    "Username",
                                    existingProfile.UserName,
                                    true
                                );
                                break;
                            case "--password":
                                existingProfile.Password = Prompt(
                                    "Password",
                                    existingProfile.Password,
                                    true
                                );
                                break;
                            case "--private-key-path":
                                existingProfile.PrivateKeyPath = Prompt(
                                    "Private Key Path",
                                    existingProfile.PrivateKeyPath,
                                    true
                                );
                                break;
                            case "--csproj-path":
                                existingProfile.CsprojPath = Prompt(
                                    "Csproj Path",
                                    existingProfile.CsprojPath,
                                    true
                                );
                                break;
                            case "--local-dir":
                                existingProfile.LocalDir = Prompt(
                                    "Local Directory",
                                    existingProfile.LocalDir,
                                    true
                                );
                                break;
                            case "--remote-dir":
                                existingProfile.RemoteDir = Prompt(
                                    "Remote Directory",
                                    existingProfile.RemoteDir,
                                    true
                                );
                                break;
                        }
                    }
                    _profileManager.FileNameChange(_profileName, existingProfile.Name);
                    _profileManager.SaveProfile(existingProfile.Name, existingProfile);
                    Message.Display(
                        $"Profile '{existingProfile.Name}' updated successfully.",
                        MessageType.Success
                    );
                }
                catch (Exception ex)
                {
                    Message.Display($"Error updating profile: {ex.Message}", MessageType.Error);
                    return Task.CompletedTask;
                }
            }

            if (!hasFields && !hasFieldsets)
            {
                var updatedProfile = PromptForProfileData(existingProfile);
                if (string.IsNullOrEmpty(updatedProfile.Name))
                {
                    return Task.CompletedTask;
                }
                _profileManager.FileNameChange(_profileName, updatedProfile.Name);
                _profileManager.SaveProfile(updatedProfile.Name, updatedProfile);
                Message.Display(
                    $"Profile '{updatedProfile.Name}' updated successfully.",
                    MessageType.Success
                );
            }
            return Task.CompletedTask;
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

        private static ProfileData PromptForProfileData(ProfileData existingProfile)
        {
            // Logic for prompting user for data to update the profile
            return existingProfile;
        }

        public static void FileNameChange(string oldName, string newName)
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

        public static string GetProfilesDirectory()
        {
            var profilesDir = Path.Combine(AppContext.BaseDirectory, "profiles");
            Directory.CreateDirectory(profilesDir);
            return profilesDir;
        }
    }
}
