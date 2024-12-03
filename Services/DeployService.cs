// Services/DeployService.cs
using System.Diagnostics;
using System.Threading.Tasks;
using CustomSftpTool.Interfaces;
using CustomSftpTool.Models;
using Serilog;

namespace CustomSftpTool.Services
{
    public class DeployService(
        IDotnetService dotnetService,
        ISshService sshService,
        ISftpService sftpService
    ) : IDeployService
    {
        private readonly IDotnetService _dotnetService = dotnetService;
        private readonly ISshService _sshService = sshService;
        private readonly ISftpService _sftpService = sftpService;

        public async Task<bool> RunDeploymentAsync(ProfileData profile, bool force)
        {
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

            _sshService.Connect();
            string? serviceStatus = _sshService
                .ExecuteCommand($"sudo systemctl is-active {profile.ServiceName}")
                ?.Trim();

            Log.Debug($"Service status command returned: {serviceStatus}");

            if (serviceStatus != "active")
            {
                Log.Error($"Expected 'active', but got '{serviceStatus}'.");
                _sshService.Disconnect();
                return false;
            }

            _sshService.ExecuteCommand($"sudo systemctl stop {profile.ServiceName}");

            _sftpService.Connect();
            await _sftpService.UploadDirectoryAsync(
                profile.LocalDir,
                profile.RemoteDir,
                profile.ExcludedFiles,
                force
            );
            _sftpService.Disconnect();

            _sshService.ExecuteCommand($"sudo systemctl start {profile.ServiceName}");
            _sshService.Disconnect();

            Log.Information("Deployment completed successfully.");
            return true;
        }
    }
}
