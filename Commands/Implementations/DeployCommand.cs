using CustomSftpTool.Interfaces;
using System.Diagnostics;

namespace CustomSftpTool.Commands.Implementations;

public class DeployCommand(
    string profileName,
    bool force,
    IProfileService profileService,
    IDeployService deployService,
    IProfileValidator profileValidator,
    ILoggerService logger
) : ICommand
{
    public async Task Execute()
    {
        // Load and validate profile
        var profile = profileService.LoadProfile(profileName);
        if (!profileValidator.Validate(profile, profileName))
        {
            return;
        }

        Debug.Assert(profile != null, "Profile should not be null after validation");
        Debug.Assert(profile.Name != null, "Profile name should not be null after validation");

        // Run deployment
        var success = await deployService.RunDeploymentAsync(profile!, force);
        if (success)
        {
            profile.LastDeploy = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            profileService.SaveProfile(profile);
            logger.LogInfo($"Deployment for profile '{profile.Name}' completed successfully.");
        }
        else
        {
            logger.LogError("Deployment failed.");
        }
    }
}