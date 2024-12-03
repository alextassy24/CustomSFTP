using CustomSftpTool.Data;
using CustomSftpTool.Interfaces;
using CustomSftpTool.Models;

namespace CustomSftpTool.Commands.Implementations
{
    public class ShowProfileCommand(IProfileService profileService, string profileName) : ICommand
    {
        private readonly IProfileService _profileService = profileService;
        private readonly string _profileName = profileName;

        public Task Execute()
        {
            var profile = _profileService.LoadProfile(_profileName);
            if (profile == null || string.IsNullOrWhiteSpace(profile.Name))
            {
                Message.Display($"Error: Profile '{_profileName}' not found.", MessageType.Error);
                return Task.CompletedTask;
            }

            DisplayProfile(profile);
            return Task.CompletedTask;
        }

        private static void DisplayProfile(ProfileData profile)
        {
            // Header
            DisplaySectionSeparator("Profile Information");

            // Profile fields
            DisplayField("Name", profile.Name);
            DisplayField("Host", profile.Host);
            DisplayField("UserName", profile.UserName);
            DisplayField("PrivateKeyPath", profile.PrivateKeyPath);
            DisplayField("CsprojPath", profile.CsprojPath);
            DisplayField("LocalDir", profile.LocalDir);
            DisplayField("RemoteDir", profile.RemoteDir);
            DisplayField("ServiceName", profile.ServiceName);
            DisplayField("Last Deploy", profile.LastDeploy);
            DisplayField("Last Backup", profile.LastBackup);

            // Excluded files
            if (profile.ExcludedFiles?.Count > 0)
            {
                DisplaySectionSeparator("Excluded Files");
                foreach (var file in profile.ExcludedFiles)
                {
                    Message.Display($"- {file}", MessageType.Info);
                }
            }

            // Footer
            DisplaySectionSeparator();
        }

        private static void DisplayField(string fieldName, string? fieldValue)
        {
            Message.Display($"{fieldName}: {fieldValue ?? "N/A"}", MessageType.Info);
        }

        private static void DisplaySectionSeparator(string? title = null)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                Message.Display("-------------------------------------", MessageType.Info);
                Message.Display(title, MessageType.Info);
                Message.Display("-------------------------------------", MessageType.Info);
            }
            else
            {
                Message.Display("-------------------------------------", MessageType.Info);
            }
        }
    }
}
