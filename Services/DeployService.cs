using CustomSftpTool.Interfaces;
using CustomSftpTool.Models;
using Serilog;

namespace CustomSftpTool.Services
{
    public class DeployService(
        IDotnetService dotnetService,
        ISshServiceFactory sshServiceFactory,
        ISftpServiceFactory sftpServiceFactory
    ) : IDeployService
    {
        private readonly IDotnetService _dotnetService = dotnetService;
        private readonly ISshServiceFactory _sshServiceFactory = sshServiceFactory;
        private readonly ISftpServiceFactory _sftpServiceFactory = sftpServiceFactory;

        public async Task<bool> RunDeploymentAsync(ProfileData profile, bool force)
        {
            var sshService = _sshServiceFactory.CreateSshService(profile);
            var sftpService = _sftpServiceFactory.CreateSftpService(profile);

            if (
                string.IsNullOrEmpty(profile.CsprojPath)
                || string.IsNullOrEmpty(profile.LocalDir)
                || string.IsNullOrEmpty(profile.RemoteDir)
                || string.IsNullOrEmpty(profile.ServiceName)
            )
            {
                Log.Error("Invalid or missing profile information.");
                return false;
            }

            Log.Information("Starting deployment...");
            Log.Information("Cleaning the solution...");

            bool cleanApp = await _dotnetService.CleanApplicationAsync(profile.CsprojPath);
            if (!cleanApp)
            {
                Log.Error("Failed to clean the application.");
                return false;
            }
            Log.Information("Cleaning complete!");

            Log.Information("Publishing the app...");
            bool publishApp = await _dotnetService.PublishApplicationAsync(
                profile.CsprojPath,
                profile.LocalDir
            );
            if (!publishApp)
            {
                Log.Error("Failed to publish the application.");
                return false;
            }
            Log.Information("Publishing complete!");

            sshService.Connect();
            string? serviceStatus = sshService
                .ExecuteCommand($"sudo systemctl is-active {profile.ServiceName}")
                ?.Trim();

            Log.Debug($"Service status command returned: {serviceStatus}");

            if (serviceStatus != "active")
            {
                Log.Error($"Expected 'active', but got '{serviceStatus}'.");
                sshService.Disconnect();
                return false;
            }

            sshService.ExecuteCommand($"sudo systemctl stop {profile.ServiceName}");

            sftpService.Connect();
            await sftpService.UploadDirectoryAsync(
                profile.LocalDir,
                profile.RemoteDir,
                profile.ExcludedFiles,
                force
            );
            sftpService.Disconnect();

            sshService.ExecuteCommand($"sudo systemctl start {profile.ServiceName}");
            sshService.Disconnect();

            Log.Information("Deployment completed successfully.");
            return true;
        }
    }
}
