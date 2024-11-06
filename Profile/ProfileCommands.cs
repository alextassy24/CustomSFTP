using System.Diagnostics;
using System.Text.Json;
using CustomSftpTool.Commands;
using CustomSftpTool.Data;
using CustomSftpTool.Models;
using CustomSftpTool.Utilities;
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
                Message.Display("Update profile data(enter for default value): ", MessageType.Info);
                existingProfile.Name = Prompt("Profile Name", existingProfile.Name, true);
                existingProfile.Host = Prompt("Host", existingProfile.Host, true);
                existingProfile.UserName = Prompt("Username", existingProfile.UserName, true);
                existingProfile.Password = Prompt("Password", existingProfile.Password, true);
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
            Message.Display("Introduce profile data(Press CTRL + C to exit): ", MessageType.Info);
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
            profileData.LastBackup = "";
            profileData.LastRestore = "";
            profileData.LastDeploy = "";

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
                Console.Write($"{message} [{defaultValue ?? null}]: ");
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
                Console.WriteLine($"Last Deploy = {profile.LastDeploy}");
                Console.WriteLine($"Last Backup = {profile.LastBackup}");
                if (profile.ExcludedFiles != null && profile.ExcludedFiles.Count > 0)
                {
                    Message.Display("-------------------------------------", MessageType.Warning);
                    Console.WriteLine($"Excluded Files:");
                    foreach (var file in profile.ExcludedFiles)
                    {
                        Message.Display($"- {file}", MessageType.Debug);
                    }
                }
                Message.Display("-------------------------------------", MessageType.Warning);
                return;
            }
            Message.Display("Error: Profile could not be found!", MessageType.Error);
        }

        public static List<string> GetAllProfiles()
        {
            string profilesDirectory = GetProfilesDirectory();

            if (!Directory.Exists(profilesDirectory))
            {
                return [];
            }

            List<string> profileNames = Directory
                .EnumerateFiles(profilesDirectory, "*.json")
                .Select(file => Path.GetFileNameWithoutExtension(file))
                .ToList();

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

        public static void EditProfile(
            string profileName,
            List<string> fields,
            Dictionary<string, string> fieldSets
        )
        {
            var existingProfile = LoadProfile(profileName);
            if (existingProfile == null || string.IsNullOrEmpty(existingProfile.Name))
            {
                Message.Display(
                    $"Error: Profile '{profileName}' could not be found.",
                    MessageType.Error
                );
                return;
            }

            bool hasFields = fields != null && fields.Count > 0;
            bool hasFieldsets = fieldSets != null && fieldSets.Count > 0;

            if (hasFieldsets && fieldSets != null)
            {
                try
                {
                    foreach (var kvp in fieldSets)
                    {
                        switch (kvp.Key.ToLower())
                        {
                            case "--name":
                                existingProfile.Name = kvp.Value;
                                break;
                            case "--host":
                                existingProfile.Host = kvp.Value;
                                break;
                            case "--user-name":
                                existingProfile.UserName = kvp.Value;
                                break;
                            case "--password":
                                existingProfile.Password = kvp.Value;
                                break;
                            case "--private-key-path":
                                existingProfile.PrivateKeyPath = kvp.Value;
                                break;
                            case "--csproj-path":
                                existingProfile.CsprojPath = kvp.Value;
                                break;
                            case "--local-dir":
                                existingProfile.LocalDir = kvp.Value;
                                break;
                            case "--remote-dir":
                                existingProfile.RemoteDir = kvp.Value;
                                break;
                            case "--service-name":
                                existingProfile.ServiceName = kvp.Value;
                                break;
                        }
                    }
                    FileNameChange(profileName, existingProfile.Name);
                    SaveProfile(existingProfile.Name, existingProfile);
                    Message.Display(
                        $"Profile '{existingProfile.Name}' updated successfully.",
                        MessageType.Success
                    );
                }
                catch (Exception ex)
                {
                    Message.Display($"Error: {ex.Message}", MessageType.Error);
                    return;
                }
            }
            if (hasFields && fields != null)
            {
                try
                {
                    foreach (var field in fields)
                    {
                        switch (field.ToLower())
                        {
                            case "--name":
                                existingProfile.Name = Prompt(
                                    "Profile Name",
                                    existingProfile.Name,
                                    true
                                );
                                break;
                            case "--host":
                                existingProfile.Host = Prompt("Host", existingProfile.Host, true);
                                break;
                            case "--service-name":
                                existingProfile.ServiceName = Prompt(
                                    "Service Name",
                                    existingProfile.ServiceName,
                                    true
                                );
                                break;
                            case "--user-name":
                                existingProfile.UserName = Prompt(
                                    "Username",
                                    existingProfile.UserName,
                                    true
                                );
                                break;
                            case "--password":
                                existingProfile.Password = Prompt(
                                    "Password",
                                    existingProfile.Password,
                                    true
                                );
                                break;
                            case "--private-key-path":
                                existingProfile.PrivateKeyPath = Prompt(
                                    "Private Key Path",
                                    existingProfile.PrivateKeyPath,
                                    true
                                );
                                break;
                            case "--csproj-path":
                                existingProfile.CsprojPath = Prompt(
                                    "Csproj Path",
                                    existingProfile.CsprojPath,
                                    true
                                );
                                break;
                            case "--local-dir":
                                existingProfile.LocalDir = Prompt(
                                    "Local Directory",
                                    existingProfile.LocalDir,
                                    true
                                );
                                break;
                            case "--remote-dir":
                                existingProfile.RemoteDir = Prompt(
                                    "Remote Directory",
                                    existingProfile.RemoteDir,
                                    true
                                );
                                break;
                        }
                    }

                    FileNameChange(profileName, existingProfile.Name);
                    SaveProfile(existingProfile.Name, existingProfile);
                    Message.Display(
                        $"Profile '{existingProfile.Name}' updated successfully.",
                        MessageType.Success
                    );
                }
                catch (Exception ex)
                {
                    Message.Display($"Error updating profile: {ex.Message}", MessageType.Error);
                }
            }

            if (!hasFields && !hasFieldsets)
            {
                var updatedProfile = PromptForProfileData(existingProfile);
                if (string.IsNullOrEmpty(updatedProfile.Name))
                {
                    return;
                }
                FileNameChange(profileName, updatedProfile.Name);
                SaveProfile(updatedProfile.Name, updatedProfile);
                Message.Display(
                    $"Profile '{updatedProfile.Name}' updated successfully.",
                    MessageType.Success
                );
            }
        }

        public static void FileNameChange(string oldName, string newName)
        {
            if (!string.Equals(newName, oldName, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(
                    $"Do you want to rename the profile from '{oldName}' to '{newName}'? (y/n)"
                );
                var response = Console.ReadLine();
                if (!string.Equals(response, "y", StringComparison.OrdinalIgnoreCase))
                {
                    Message.Display("Profile renaming canceled.", MessageType.Warning);
                    return;
                }

                // Rename the JSON file
                var profilesDir = GetProfilesDirectory();
                var oldProfilePath = Path.Combine(profilesDir, $"{oldName}.json");
                var newProfilePath = Path.Combine(profilesDir, $"{newName}.json");

                try
                {
                    File.Move(oldProfilePath, newProfilePath);
                    Message.Display(
                        $"Profile renamed from '{oldName}' to '{newName}'.",
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
                    return;
                }
                Message.Display($"Error: Profile '{profile}' not found.", MessageType.Error);
            }
            catch (Exception ex)
            {
                Message.Display($"Error: Deleting profile failed: {ex.Message}", MessageType.Error);
            }
        }

        public static async Task DeployUsingProfile(string profileName, bool force = false)
        {
            var profile = LoadProfile(profileName);
            if (profile == null || profileName == null || profile.Name == null)
            {
                Message.Display($"Error: Profile '{profileName}' not found.", MessageType.Error);
                return;
            }

            if (await RunDeployment(profile, force))
            {
                profile.LastDeploy = DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
                SaveProfile(profile.Name, profile);
                return;
            }

            Message.Display($"Error: Deploy failed.", MessageType.Error);
        }

        public static async Task<bool> RunDeployment(ProfileData profile, bool force = false)
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
                if (
                    profile == null
                    || string.IsNullOrEmpty(profile.Host)
                    || string.IsNullOrEmpty(profile.UserName)
                    || string.IsNullOrEmpty(profile.PrivateKeyPath)
                    || string.IsNullOrEmpty(profile.CsprojPath)
                    || string.IsNullOrEmpty(profile.LocalDir)
                    || string.IsNullOrEmpty(profile.RemoteDir)
                )
                {
                    Log.Error(
                        "Profile data is null. Please check Host, UserName, PrivateKeyPath, CsprojPath, RemoteDir and LocalDir properties in the profile data."
                    );
                    return false;
                }

                Log.Information("Starting deployment...");
                Stopwatch totalStopwatch = Stopwatch.StartNew();
                Log.Information("Cleaning the application...");
                Stopwatch cleanStopwatch = Stopwatch.StartNew();

                bool cleanApp = await DotnetCommands.CleanApplicationAsync(profile.CsprojPath);
                cleanStopwatch.Stop();
                if (!cleanApp)
                {
                    Log.Error("Failed to clean the application.");
                    return false;
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
                    return false;
                }
                Log.Information(
                    "Publishing completed in {Elapsed} seconds.",
                    publishStopwatch.Elapsed.TotalSeconds
                );

                Log.Information("Connecting to SSH...");
                using SshClient sshClient = SshCommands.CreateSshClient(
                    profile.Host,
                    profile.UserName,
                    profile.Password,
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
                    return false;
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
                    return false;
                }

                Log.Information($"Service {profile.ServiceName} is stopped.");
                Log.Information("Transferring newer files...");

                using SftpClient sftpClient = new(sshClient.ConnectionInfo);
                sftpClient.Connect();

                bool uploadFiles = await SftpCommands.UploadDirectoryAsync(
                    sftpClient,
                    profile.LocalDir,
                    profile.RemoteDir,
                    profile.ExcludedFiles ?? [],
                    force
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
                    return false;
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
                    return false;
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
                    $"Deployment completed in {totalStopwatch.Elapsed.TotalSeconds} seconds."
                );
                sshClient.Disconnect();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred during deployment.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
            return false;
        }
    }
}
