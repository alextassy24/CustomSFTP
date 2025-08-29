using CustomSftpTool.Interfaces;
using CustomSftpTool.Models;
using Serilog;

namespace CustomSftpTool.Services;

public class DeployService(
    IDotnetService dotnetService,
    ISshServiceFactory sshServiceFactory,
    ISftpServiceFactory sftpServiceFactory
) : IDeployService
{
    public async Task<bool> RunDeploymentAsync(ProfileData profile, bool force)
    {
        // Validate profile early
        if (!ValidateProfile(profile))
        {
            return false;
        }

        try
        {
            Log.Information("Starting deployment for profile '{ProfileName}'...", profile.Name);

            // Step 1: Clean and publish
            if (!await CleanAndPublishAsync(profile))
            {
                return false;
            }

            // Step 2: Handle service and upload
            return await DeployToRemoteAsync(profile, force);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Deployment failed for profile '{ProfileName}'", profile.Name);
            return false;
        }
    }

    private static bool ValidateProfile(ProfileData profile)
    {
        var requiredFields = new Dictionary<string, string?>
        {
            [nameof(profile.CsprojPath)] = profile.CsprojPath,
            [nameof(profile.LocalDir)] = profile.LocalDir,
            [nameof(profile.RemoteDir)] = profile.RemoteDir,
            [nameof(profile.ServiceName)] = profile.ServiceName
        };

        var missingFields = requiredFields
            .Where(kvp => string.IsNullOrEmpty(kvp.Value))
            .Select(kvp => kvp.Key)
            .ToList();

        if (missingFields.Count > 0)
        {
            Log.Error("Missing required profile fields: {MissingFields}", string.Join(", ", missingFields));
            return false;
        }

        return true;
    }

    private async Task<bool> CleanAndPublishAsync(ProfileData profile)
    {
        Log.Information("Cleaning the solution...");
        if (!await dotnetService.CleanApplicationAsync(profile.CsprojPath!))
        {
            Log.Error("Failed to clean the application.");
            return false;
        }
        Log.Information("Cleaning complete!");

        Log.Information("Publishing the application...");
        if (!await dotnetService.PublishApplicationAsync(profile.CsprojPath!, profile.LocalDir!))
        {
            Log.Error("Failed to publish the application.");
            return false;
        }
        Log.Information("Publishing complete!");

        return true;
    }

    private async Task<bool> DeployToRemoteAsync(ProfileData profile, bool force)
    {
        var sshService = sshServiceFactory.CreateSshService(profile);
        var sftpService = sftpServiceFactory.CreateSftpService(profile);

        try
        {
            // Connect to services
            sshService.Connect();
            
            // Check and stop service if running
            if (!await StopServiceIfActiveAsync(sshService, profile.ServiceName!))
            {
                return false;
            }

            // Upload files
            sftpService.Connect();
            var uploadSuccess = await sftpService.UploadDirectoryAsync(
                profile.LocalDir!,
                profile.RemoteDir!,
                profile.ExcludedFiles,
                force
            );

            if (!uploadSuccess)
            {
                Log.Error("Failed to upload files. Attempting to restart service...");
                await StartServiceAsync(sshService, profile.ServiceName!);
                return false;
            }

            // Start service back up
            return await StartServiceAsync(sshService, profile.ServiceName!);
        }
        finally
        {
            // Ensure connections are closed
            try { sftpService.Disconnect(); } catch { /* Ignore cleanup errors */ }
            try { sshService.Disconnect(); } catch { /* Ignore cleanup errors */ }
        }
    }

    private static async Task<bool> StopServiceIfActiveAsync(ISshService sshService, string serviceName)
    {
        var serviceStatus = sshService.ExecuteCommand($"sudo systemctl is-active {serviceName}")?.Trim();
        Log.Debug("Service '{ServiceName}' status: {Status}", serviceName, serviceStatus);

        if (serviceStatus == "active")
        {
            Log.Information("Stopping service '{ServiceName}'...", serviceName);
            var stopResult = sshService.ExecuteCommand($"sudo systemctl stop {serviceName}");
            
            // Wait a moment for the service to stop
            await Task.Delay(2000);
            
            // Verify service stopped
            var newStatus = sshService.ExecuteCommand($"sudo systemctl is-active {serviceName}")?.Trim();
            if (newStatus == "active")
            {
                Log.Error("Failed to stop service '{ServiceName}'", serviceName);
                return false;
            }
            
            Log.Information("Service '{ServiceName}' stopped successfully.", serviceName);
        }
        else if (serviceStatus == "inactive" || serviceStatus == "failed")
        {
            Log.Information("Service '{ServiceName}' is not running (status: {Status})", serviceName, serviceStatus);
        }
        else
        {
            Log.Warning("Service '{ServiceName}' has unexpected status: {Status}", serviceName, serviceStatus);
        }

        return true;
    }

    private static async Task<bool> StartServiceAsync(ISshService sshService, string serviceName)
    {
        Log.Information("Starting service '{ServiceName}'...", serviceName);
        sshService.ExecuteCommand($"sudo systemctl start {serviceName}");
        
        // Wait for service to start
        await Task.Delay(3000);
        
        // Verify service started
        var finalStatus = sshService.ExecuteCommand($"sudo systemctl is-active {serviceName}")?.Trim();
        
        if (finalStatus == "active")
        {
            Log.Information("Service '{ServiceName}' started successfully.", serviceName);
            return true;
        }
        else
        {
            Log.Error("Service '{ServiceName}' failed to start. Final status: {Status}", serviceName, finalStatus);
            
            // Get more detailed error information
            var statusOutput = sshService.ExecuteCommand($"sudo systemctl status {serviceName}");
            if (!string.IsNullOrEmpty(statusOutput))
            {
                Log.Error("Service status details: {StatusOutput}", statusOutput);
            }
            
            return false;
        }
    }
}