using System.Diagnostics;
using System.Text.Json;
using CustomSftpTool.Commands;
using CustomSftpTool.Data;
using CustomSftpTool.Models;
using Renci.SshNet;
using Serilog;

namespace CustomSftpTool.Profile
{
    public static class ProfileCommands
    {
        public static void AddProfile()
        {
            var profileData = PromptForProfileData();
            SaveProfile($"{profileData.Name}", profileData);
        }

        public static ProfileData PromptForProfileData(ProfileData? existingProfile = null)
        {
            if (existingProfile != null)
            {
                Console.WriteLine("Update profile data(enter for default value): ");
                existingProfile.Name = Prompt("Profile Name", existingProfile.Name, true);
                existingProfile.Host = Prompt("Host", existingProfile.Host, true);
                existingProfile.UserName = Prompt("Username", existingProfile.UserName, true);
                existingProfile.PrivateKeyPath = Prompt(
                    "Private Key Path",
                    existingProfile.PrivateKeyPath,
                    true
                );
                existingProfile.CsprojPath = Prompt(
                    "Csproj Path",
                    existingProfile.CsprojPath,
                    true
                );
                existingProfile.LocalDir = Prompt(
                    "Local Directory",
                    existingProfile.LocalDir,
                    true
                );
                existingProfile.RemoteDir = Prompt(
                    "Remote Directory",
                    existingProfile.RemoteDir,
                    true
                );
                existingProfile.ServiceName = Prompt(
                    "Service Name",
                    existingProfile.ServiceName,
                    true
                );
                existingProfile.ExcludedFiles = PromptForExcludedFiles(
                    existingProfile.ExcludedFiles
                );

                return existingProfile;
            }

            var profileData = new ProfileData();
            Console.WriteLine("Introduce profile data: ");
            profileData.Name = Prompt("Profile Name", profileData.Name, true);
            profileData.Host = Prompt("Host", profileData.Host, true);
            profileData.UserName = Prompt("Username", profileData.UserName, true);
            profileData.PrivateKeyPath = Prompt(
                "Private Key Path",
                profileData.PrivateKeyPath,
                true
            );
            profileData.CsprojPath = Prompt("Csproj Path", profileData.CsprojPath, true);
            profileData.LocalDir = Prompt("Local Directory", profileData.LocalDir, true);
            profileData.RemoteDir = Prompt("Remote Directory", profileData.RemoteDir, true);
            profileData.ServiceName = Prompt("Service Name", profileData.ServiceName, true);
            // After other prompts
            profileData.ExcludedFiles = PromptForExcludedFiles();

            return profileData;
        }

        public static string Prompt(
            string message,
            string? defaultValue = null,
            bool isRequired = false
        )
        {
            while (true)
            {
                if (!string.IsNullOrEmpty(defaultValue))
                {
                    Console.Write($"{message} [{defaultValue}]: ");
                }
                else
                {
                    Console.Write($"{message}: ");
                }

                string? input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    if (isRequired && string.IsNullOrEmpty(defaultValue))
                    {
                        Message.Display($"{message} is required.", MessageType.Error);
                        continue;
                    }
                    else if (!string.IsNullOrEmpty(defaultValue))
                        return defaultValue;
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

        public static string GetProfilesDirectory()
        {
            var profilesDir = Path.Combine(AppContext.BaseDirectory, "profiles");
            Directory.CreateDirectory(profilesDir);
            return profilesDir;
        }

        public static void SaveProfile(string profileName, ProfileData profileData)
        {
            var profilesDir = GetProfilesDirectory();
            var profilePath = Path.Combine(profilesDir, $"{profileName}.json");
            var json = JsonSerializer.Serialize(
                profileData,
                new JsonSerializerOptions { WriteIndented = true }
            );
            File.WriteAllText(profilePath, json);
        }

        public static ProfileData? LoadProfile(string profileName)
        {
            var profilesDir = GetProfilesDirectory();
            var profilePath = Path.Combine(profilesDir, $"{profileName}.json");
            if (!File.Exists(profilePath))
            {
                return null;
            }
            var json = File.ReadAllText(profilePath);
            var profileData = JsonSerializer.Deserialize<ProfileData>(json);
            return profileData;
        }

        public static void ShowProfile(ProfileData profile)
        {
            if (profile != null)
            {
                Console.WriteLine("Showing profile data:");
                Message.Display("-------------------------------------", MessageType.Warning);
                Console.WriteLine($"Name = {profile.Name}");
                Console.WriteLine($"Host = {profile.Host}");
                Console.WriteLine($"UserName = {profile.UserName}");
                Console.WriteLine($"PrivateKeyPath = {profile.PrivateKeyPath}");
                Console.WriteLine($"CsprojPath = {profile.CsprojPath}");
                Console.WriteLine($"LocalDir = {profile.LocalDir}");
                Console.WriteLine($"RemoteDir = {profile.RemoteDir}");
                Console.WriteLine($"ServiceName = {profile.ServiceName}");
                if (profile.ExcludedFiles != null && profile.ExcludedFiles.Count > 0)
                {
                    Message.Display("Excluded Files:", MessageType.Debug);
                    foreach (var file in profile.ExcludedFiles)
                    {
                        Message.Display($"- {file}", MessageType.Debug);
                    }
                }
                Message.Display("-------------------------------------", MessageType.Warning);
            }
            else
            {
                Message.Display("Profile could not be found!", MessageType.Error);
            }
        }

        public static List<string> GetAllProfiles()
        {
            var profilesDir = GetProfilesDirectory();
            var profileFiles = Directory.GetFiles(profilesDir, "*.json");
            var profileNames = new List<string>();
            foreach (var file in profileFiles)
            {
                var profileName = Path.GetFileNameWithoutExtension(file);
                profileNames.Add(profileName);
            }
            return profileNames;
        }

        public static void ListProfiles()
        {
            var profileNames = GetAllProfiles();
            Message.Display("-------------------------------------", MessageType.Warning);
            Console.WriteLine("Available Profiles:");
            foreach (var profileName in profileNames)
            {
                Message.Display($"- {profileName}", MessageType.Success);
            }

            Message.Display("-------------------------------------", MessageType.Warning);
        }

        public static void EditProfile(string profileName)
        {
            var existingProfile = LoadProfile(profileName);
            if (existingProfile == null)
            {
                Message.Display($"Error: Profile '{profileName}' not found.", MessageType.Error);
                return;
            }

            var updatedProfile = PromptForProfileData(existingProfile);

            // Check if the profile name has been changed
            if (
                !string.Equals(updatedProfile.Name, profileName, StringComparison.OrdinalIgnoreCase)
            )
            {
                // Check if a profile with the new name already exists
                if (LoadProfile(updatedProfile.Name) != null)
                {
                    Message.Display(
                        $"Error: A profile with the name '{updatedProfile.Name}' already exists.",
                        MessageType.Error
                    );
                    return;
                }

                // Ask for confirmation
                Console.WriteLine(
                    $"Do you want to rename the profile from '{profileName}' to '{updatedProfile.Name}'? (y/n)"
                );
                var response = Console.ReadLine();
                if (!string.Equals(response, "y", StringComparison.OrdinalIgnoreCase))
                {
                    Message.Display($"Profile renaming canceled.", MessageType.Warning);
                    return;
                }

                // Rename the JSON file
                var profilesDir = GetProfilesDirectory();
                var oldProfilePath = Path.Combine(profilesDir, $"{profileName}.json");
                var newProfilePath = Path.Combine(profilesDir, $"{updatedProfile.Name}.json");

                try
                {
                    File.Move(oldProfilePath, newProfilePath);
                    Message.Display(
                        $"Profile renamed from '{profileName}' to '{updatedProfile.Name}'.",
                        MessageType.Success
                    );
                }
                catch (Exception ex)
                {
                    Message.Display(
                        $"Error renaming profile file: {ex.Message}",
                        MessageType.Error
                    );
                    return;
                }
            }

            // Save the updated profile data
            SaveProfile(updatedProfile.Name, updatedProfile);
            Message.Display(
                $"Profile '{updatedProfile.Name}' updated successfully.",
                MessageType.Success
            );
        }

        public static void RemoveProfile(string profile)
        {
            try
            {
                var profilesDir = GetProfilesDirectory();
                var profilePath = Path.Combine(profilesDir, $"{profile}.json");
                if (File.Exists(profilePath))
                {
                    File.Delete(profilePath);
                    Message.Display(
                        $"Profile '{profile}' deleted successfully.",
                        MessageType.Success
                    );
                }
                else
                {
                    Message.Display($"Profile '{profile}' not found.", MessageType.Error);
                }
            }
            catch (Exception ex)
            {
                Message.Display($"Error deleting profile: {ex.Message}", MessageType.Error);
            }
        }

        public static async Task DeployUsingProfile(string profileName)
        {
            var profile = LoadProfile(profileName);
            if (profile == null)
            {
                Message.Display($"Error: Profile '{profileName}' not found.", MessageType.Error);
                return;
            }

            // Call your deployment logic here, passing the profile data
            await RunDeployment(profile);
        }

        public static async Task RunDeployment(ProfileData profile)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(
                    outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .WriteTo.File("logs\\deployment.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                Log.Information("Starting deployment...");

                Stopwatch totalStopwatch = Stopwatch.StartNew();
                Log.Information("Cleaning the application...");
                Stopwatch cleanStopwatch = Stopwatch.StartNew();

                bool cleanApp = await DotnetCommands.CleanApplicationAsync(profile.CsprojPath);
                cleanStopwatch.Stop();
                if (!cleanApp)
                {
                    Log.Error("Failed to clean the application.");
                    return;
                }

                Log.Information(
                    "Cleaning completed in {Elapsed} seconds.",
                    cleanStopwatch.Elapsed.TotalSeconds
                );

                Log.Information("Publishing the application...");
                Stopwatch publishStopwatch = Stopwatch.StartNew();

                bool publishApp = await DotnetCommands.PublishApplicationAsync(
                    profile.CsprojPath,
                    profile.LocalDir
                );
                publishStopwatch.Stop();

                if (!publishApp)
                {
                    Log.Error("Failed to publish the application.");
                    return;
                }
                Log.Information(
                    "Publishing completed in {Elapsed} seconds.",
                    publishStopwatch.Elapsed.TotalSeconds
                );

                Log.Information("Connecting to SSH...");
                using SshClient sshClient = SshCommands.CreateSshClient(
                    profile.Host,
                    profile.UserName,
                    profile.PrivateKeyPath
                );

                sshClient.Connect();

                Log.Information("Checking service status...");
                string? serviceStatus = SshCommands.ExecuteCommand(
                    sshClient,
                    $"sudo systemctl is-active {profile.ServiceName}"
                );

                if (string.IsNullOrEmpty(serviceStatus) || serviceStatus.Trim() != "active")
                {
                    Log.Warning(
                        $"Service {profile.ServiceName} is not active or failed to check status."
                    );
                    sshClient.Disconnect();
                    return;
                }

                Log.Information($"Service {profile.ServiceName} is active.");
                Log.Information($"Stopping service {profile.ServiceName}.");

                string? stopServiceResult = SshCommands.ExecuteCommand(
                    sshClient,
                    $"sudo systemctl stop {profile.ServiceName}"
                );

                if (stopServiceResult == null)
                {
                    Log.Error($"Failed to stop service {profile.ServiceName}.");
                    sshClient.Disconnect();
                    return;
                }

                Log.Information($"Service {profile.ServiceName} is stopped.");
                Log.Information("Transferring newer files...");

                using SftpClient sftpClient = new(sshClient.ConnectionInfo);
                sftpClient.Connect();

                bool uploadFiles = await SftpCommands.UploadDirectoryAsync(
                    sftpClient,
                    profile.LocalDir,
                    profile.RemoteDir,
                    profile.ExcludedFiles ?? []
                );

                sftpClient.Disconnect();

                if (!uploadFiles)
                {
                    Log.Error("Failed to transfer newer files.");
                    Log.Information($"Restarting service {profile.ServiceName}...");
                    SshCommands.ExecuteCommand(
                        sshClient,
                        $"sudo systemctl start {profile.ServiceName}"
                    );
                    sshClient.Disconnect();
                    return;
                }

                Log.Information("Transferred newer files.");
                Log.Information($"Restarting service {profile.ServiceName}...");
                string? startServiceResult = SshCommands.ExecuteCommand(
                    sshClient,
                    $"sudo systemctl start {profile.ServiceName}"
                );

                if (startServiceResult == null)
                {
                    Log.Error($"Failed to start service {profile.ServiceName}.");
                    sshClient.Disconnect();
                    return;
                }

                Log.Information($"Service {profile.ServiceName} is started.");
                Log.Information("Checking its status...");
                await Task.Delay(2000);

                serviceStatus = SshCommands.ExecuteCommand(
                    sshClient,
                    $"sudo systemctl is-active {profile.ServiceName}"
                );

                if (serviceStatus != null && serviceStatus.Trim() == "active")
                {
                    Log.Information($"Service {profile.ServiceName} is active again.");
                    Log.Information(ConsoleColors.Green("Deployment completed successfully!"));
                }
                else
                {
                    Log.Error($"Service {profile.ServiceName} failed to start.");
                }
                totalStopwatch.Stop();
                Log.Information(
                    "Deployment completed in {Elapsed} seconds.",
                    totalStopwatch.Elapsed.TotalSeconds
                );
                sshClient.Disconnect();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred during deployment.");
            }
            finally
            {
                Log.Information("Deployment process finished.");
                Log.CloseAndFlush();
            }
        }
    }
}
