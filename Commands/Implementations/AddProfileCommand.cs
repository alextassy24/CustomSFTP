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

        private static ProfileData PromptForProfileData()
        {
            var profileData = new ProfileData();
            
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
            profileData.ExcludedFiles = PromptForExcludedFiles();

            return profileData;
        }

        private static string Prompt(string message, string? defaultValue = null, bool isRequired = false)
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

        public static List<string> PromptForExcludedFiles(List<string>? existingExclusions = null)
        {
            var exclusions = existingExclusions ?? [];

            Console.WriteLine("Enter files or directories to exclude (leave empty to finish):");
            Console.WriteLine("Current exclusions:");
            foreach (var exclusion in exclusions)
            {
                Console.WriteLine($"- {exclusion}");
            }
            Console.WriteLine("Add new exclusions (leave empty to finish):");

            while (true)
            {
                Message.Display("Exclude: ", MessageType.Debug);
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    break;
                }
                exclusions.Add(input);
            }

            return exclusions;
        }
    }
}
