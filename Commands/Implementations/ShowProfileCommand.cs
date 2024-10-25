// Commands/Implementations/ShowProfileCommand.cs
using CustomSftpTool.Data;
using CustomSftpTool.Interfaces;
using CustomSftpTool.Models;

namespace CustomSftpTool.Commands.Implementations
{
    public class ShowProfileCommand(IProfileManager profileManager, string profileName) : ICommand
    {
        private readonly IProfileManager _profileManager = profileManager;
        private readonly string _profileName = profileName;

        public Task Execute()
        {
            var profile = _profileManager.LoadProfile(_profileName);
            if (profile == null || string.IsNullOrEmpty(profile.Name))
            {
                Message.Display($"Error: Profile '{_profileName}' not found.", MessageType.Error);
                return Task.CompletedTask;
            }

            DisplayProfile(profile);
            return Task.CompletedTask;
        }

        private static void DisplayProfile(ProfileData profile)
        {
            Message.Display("-------------------------------------", MessageType.Info);
            Message.Display($"Name: {profile.Name}", MessageType.Success);
            Message.Display($"Host: {profile.Host}", MessageType.Info);
            Message.Display($"UserName: {profile.UserName}", MessageType.Info);
            Message.Display($"PrivateKeyPath: {profile.PrivateKeyPath}", MessageType.Info);
            Message.Display($"CsprojPath: {profile.CsprojPath}", MessageType.Info);
            Message.Display($"LocalDir: {profile.LocalDir}", MessageType.Info);
            Message.Display($"RemoteDir: {profile.RemoteDir}", MessageType.Info);
            Message.Display($"ServiceName: {profile.ServiceName}", MessageType.Info);
            Message.Display($"Last Deploy: {profile.LastDeploy}", MessageType.Info);
            Message.Display($"Last Backup: {profile.LastBackup}", MessageType.Info);

            if (profile.ExcludedFiles?.Count > 0)
            {
                Message.Display("-------------------------------------", MessageType.Info);
                Message.Display("Excluded Files:", MessageType.Info);
                foreach (var file in profile.ExcludedFiles)
                {
                    Message.Display($"- {file}", MessageType.Info);
                }
            }

            Message.Display("-------------------------------------", MessageType.Info);
        }
    }
}
