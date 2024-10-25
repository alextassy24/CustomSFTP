// Commands/Implementations/AddProfileCommand.cs
using CustomSftpTool.Data;
using CustomSftpTool.Interfaces;
using CustomSftpTool.Models;

namespace CustomSftpTool.Commands.Implementations
{
    public class AddProfileCommand(IProfileManager profileManager) : ICommand
    {
        private readonly IProfileManager _profileManager = profileManager;

        public Task Execute()
        {
            var profileData = PromptForProfileData();
            _profileManager.AddProfile(profileData);
            Message.Display(
                $"Profile '{profileData.Name}' added successfully.",
                MessageType.Success
            );
            return Task.CompletedTask;
        }

        private ProfileData PromptForProfileData()
        {
            var profileData = new ProfileData();
            // Add logic to prompt the user for profile data (same as in ProfileCommands.cs)
            profileData.Name = Prompt("Profile Name", profileData.Name, true);
            profileData.Host = Prompt("Host", profileData.Host, true);
            profileData.UserName = Prompt("Username", profileData.UserName, true);
            profileData.Password = Prompt("Password", profileData.Password, true);
            profileData.PrivateKeyPath = Prompt(
                "Private Key Path",
                profileData.PrivateKeyPath,
                true
            );
            profileData.CsprojPath = Prompt("Csproj Path", profileData.CsprojPath, true);
            profileData.LocalDir = Prompt("Local Directory", profileData.LocalDir, true);
            profileData.RemoteDir = Prompt("Remote Directory", profileData.RemoteDir, true);
            profileData.ServiceName = Prompt("Service Name", profileData.ServiceName, true);

            return profileData;
        }

        private string Prompt(string message, string? defaultValue = null, bool isRequired = false)
        {
            // Logic to prompt for input
            while (true)
            {
                Console.Write($"{message} [{defaultValue ?? string.Empty}]: ");
                string? input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    if (isRequired && string.IsNullOrEmpty(defaultValue))
                    {
                        Message.Display($"{message} is required.", MessageType.Error);
                        continue;
                    }
                    else if (!string.IsNullOrWhiteSpace(defaultValue))
                    {
                        return defaultValue;
                    }
                }

                return input ?? string.Empty;
            }
        }
    }
}
