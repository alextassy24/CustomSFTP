using CustomSftpTool.Data;
using CustomSftpTool.Interfaces;
using CustomSftpTool.Models;

namespace CustomSftpTool.Commands.Implementations
{
    public class DeployCommand(
        string profileName,
        bool force,
        IProfileManager profileManager,
        IDeployService deployService
    ) : ICommand
    {
        private readonly string _profileName = profileName;
        private readonly bool _force = force;
        private readonly IProfileManager _profileManager = profileManager;
        private readonly IDeployService _deployService = deployService;

        public async Task Execute()
        {
            var profile = _profileManager.LoadProfile(_profileName);
            if (profile == null || string.IsNullOrEmpty(profile.Name))
            {
                Message.Display($"Error: Profile '{_profileName}' not found.", MessageType.Error);
                return;
            }

            bool success = await _deployService.RunDeploymentAsync(profile, _force);
            if (success)
            {
                profile.LastDeploy = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                _profileManager.SaveProfile(profile.Name, profile);
            }
            else
            {
                Message.Display("Deployment failed.", MessageType.Error);
            }
        }
    }
}
