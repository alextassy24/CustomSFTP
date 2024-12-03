using CustomSftpTool.Interfaces;
using CustomSftpTool.Models;
using System.Diagnostics;

namespace CustomSftpTool.Commands.Implementations
{
    public class DeployCommand(
        string profileName,
        bool force,
        IProfileService profileService,
        IDeployService deployService,
        IProfileValidator profileValidator,
        ILoggerService logger
    ) : ICommand
    {
        private readonly string _profileName = profileName;
        private readonly bool _force = force;
        private readonly IProfileService _profileService = profileService;
        private readonly IDeployService _deployService = deployService;
        private readonly IProfileValidator _profileValidator = profileValidator;
        private readonly ILoggerService _logger = logger;

        public async Task Execute()
        {
            // Load and validate profile
            var profile = _profileService.LoadProfile(_profileName);
            if (!_profileValidator.Validate(profile, _profileName))
            {
                return;
            }

            Debug.Assert(profile != null, "Profile should not be null after validation");
            Debug.Assert(profile.Name != null, "Profile name should not be null after validation");

            // Run deployment
            bool success = await _deployService.RunDeploymentAsync(profile!, _force);
            if (success)
            {
                profile.LastDeploy = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                _profileService.SaveProfile(profile);
                _logger.LogInfo($"Deployment for profile '{profile.Name}' completed successfully.");
            }
            else
            {
                _logger.LogError("Deployment failed.");
            }
        }
    }
}
