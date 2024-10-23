using CustomSftpTool.Commands;
using CustomSftpTool.Data;
using CustomSftpTool.Models;
using CustomSftpTool.Profile;
using Renci.SshNet;
using Serilog;
using ShellProgressBar;

namespace CustomSftpTool
{
    internal class Program
    {
        public static async Task Main(string[] args)
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
                if (args.Length == 0)
                {
                    ShowHelp();
                    return;
                }

                string profileName = string.Empty;
                string fieldToEdit = string.Empty;
                string newValue = string.Empty;

                string command = args[0].ToLower();
                var options = args.Skip(1).ToList();

                bool force = options.Contains("--force");
                options.Remove("--force");

                if (args.Length > 1)
                {
                    profileName = string.Join(
                        ' ',
                        args.Skip(1).Where(static arg => arg != "--force")
                    );
                }

                switch (command)
                {
                    case "deploy":
                        if (!string.IsNullOrEmpty(profileName))
                        {
                            await ProfileCommands.DeployUsingProfile(profileName, force);
                        }
                        else
                        {
                            Message.Display("Error: Missing profile name.", MessageType.Error);
                            Message.Display(
                                "Usage: customSFTP deploy <ProfileName> [--force]",
                                MessageType.Warning
                            );
                        }
                        break;
                    case "add-profile":
                        ProfileCommands.AddProfile();
                        break;
                    case "list-profiles":
                        ProfileCommands.ListProfiles();
                        break;
                    case "show-profile":
                        CheckProfileName(ShowProfile, profileName, "show-profile");
                        break;
                    case "edit-profile":
                        if (options.Count > 0)
                        {
                            profileName = options[0];
                            options.RemoveAt(0);
                        }
                        else
                        {
                            Message.Display("Error: Missing profile name.", MessageType.Error);
                            return;
                        }

                        List<string> fields = [];
                        Dictionary<string, string> fieldSets = [];

                        while (options.Count > 0)
                        {
                            if (options[0].StartsWith("--"))
                            {
                                string key = options[0];
                                options.RemoveAt(0);

                                if (options.Count > 0 && !options[0].StartsWith("--"))
                                {
                                    string value = options[0];
                                    options.RemoveAt(0);
                                    fieldSets.Add(key, value);
                                }
                                else
                                {
                                    fields.Add(key);
                                }
                            }
                            else
                            {
                                options.RemoveAt(0);
                            }
                        }
                        ProfileCommands.EditProfile(profileName, fields, fieldSets);
                        break;
                    case "remove-profile":
                        CheckProfileName(
                            ProfileCommands.RemoveProfile,
                            profileName,
                            "remove-profile"
                        );
                        break;
                    case "stop-service":
                    case "start-service":
                    case "restart-service":
                    case "check-service":
                    case "status-service":
                        string action = command switch
                        {
                            "stop-service" => "stop",
                            "start-service" => "start",
                            "restart-service" => "restart",
                            "check-service" => "is-active",
                            "status-service" => "status",
                            _ => throw new InvalidOperationException("Invalid service command.")
                        };
                        ExecuteServiceCommand(profileName, action);
                        break;
                    case "backup":
                        CheckProfileName(CreateBackup, profileName, "backup");
                        break;
                    case "help":
                        ShowHelp();
                        break;
                    default:
                        ShowHelp();
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, ConsoleColors.Red("Error: An unhandled exception occurred."));
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static void ShowHelp()
        {
            Message.Display("Description:", MessageType.Warning);
            Console.WriteLine("  Custom Deployment Tool");

            Message.Display("\nUsage:", MessageType.Warning);
            Console.WriteLine("  customSFTP [command] [options]");

            Message.Display("\nCommands:", MessageType.Warning);
            PrintOption("deploy <ProfileName>", "Deploy using a profile");
            PrintOption("add-profile", "Add a new profile");
            PrintOption("list-profiles", "List all available profiles");
            PrintOption("show-profile <ProfileName>", "Show details of a profile");
            PrintOption("edit-profile <ProfileName>", "Edit an existing profile");
            PrintOption("remove-profile <ProfileName>", "Remove a profile");
            PrintOption("help", "Show help and usage information");

            Message.Display("\nOptions (for use with edit-profile):", MessageType.Warning);
            PrintOption("--host <host>", "The SSH host");
            PrintOption("--user-name <username>", "The SSH username");
            PrintOption("--private-Key-Path <path>", "Path to the private SSH key");
            PrintOption("--csproj-Path <path>", "Path to the .csproj file");
            PrintOption("--local-Dir <path>", "Local directory for the build output");
            PrintOption("--remote-Dir <path>", "Remote directory to deploy to");
            PrintOption("--service-Name <name>", "Name of the service to restart");
        }

        private static void PrintOption(string option, string description)
        {
            Console.WriteLine($"  {option, -35} {description}");
        }

        private static void ExecuteServiceCommand(string profileName, string action)
        {
            if (!string.IsNullOrEmpty(profileName))
            {
                ProfileData? profile = ProfileCommands.LoadProfile(profileName);
                // Check if profile is not null before validating
                if (profile == null)
                {
                    DisplayProfileError(
                        $"Error: Profile '{profileName}' could not be found.",
                        $"{action}"
                    );
                    return;
                }
                ExecuteSystemCtlCommand(profile, action);
                return;
            }
            Message.Display("Error: Missing profile name.", MessageType.Error);
            Message.Display(
                $"Usage: customSFTP {action}-service <ProfileName>",
                MessageType.Warning
            );
        }

        private static void ExecuteSystemCtlCommand(ProfileData profile, string action)
        {
            if (
                profile == null
                || string.IsNullOrEmpty(profile.Host)
                || string.IsNullOrEmpty(profile.UserName)
            )
            {
                DisplayProfileError(
                    "Error: Profile is missing required SSH host, username, password/private key path, or service name.",
                    $"{action}"
                );
                return;
            }

            using SshClient sshClient = SshCommands.CreateSshClient(
                profile.Host,
                profile.UserName,
                profile.Password ?? string.Empty,
                profile.PrivateKeyPath ?? string.Empty
            );

            try
            {
                sshClient.Connect();
                if (sshClient.IsConnected)
                {
                    SshCommands.ExecuteCommand(
                        sshClient,
                        $"sudo systemctl {action} {profile.ServiceName}"
                    );
                }
                else
                {
                    Message.Display("Error: Unable to connect to SSH.", MessageType.Error);
                }
            }
            catch (Exception ex)
            {
                Message.Display(
                    $"Error executing systemctl command: {ex.Message}",
                    MessageType.Error
                );
            }
            finally
            {
                if (sshClient.IsConnected)
                {
                    sshClient.Disconnect();
                }
            }
        }

        private static void CreateBackup(string profileName)
        {
            try
            {
                ProfileData? profile = ProfileCommands.LoadProfile(profileName);
                if (
                    string.IsNullOrEmpty(profileName)
                    || profile == null
                    || string.IsNullOrEmpty(profile.Host)
                    || string.IsNullOrEmpty(profile.UserName)
                    || string.IsNullOrEmpty(profile.RemoteDir)
                    || string.IsNullOrEmpty(profile.ServiceName)
                )
                {
                    DisplayProfileError(
                        "Missing profile data or required fields are null or empty.",
                        "backup"
                    );
                    return;
                }

                using SshClient sshClient = SshCommands.CreateSshClient(
                    profile.Host,
                    profile.UserName,
                    profile.Password,
                    profile.PrivateKeyPath
                );

                try
                {
                    sshClient.Connect();
                    if (!sshClient.IsConnected)
                    {
                        Message.Display("Error: Unable to connect to SSH.", MessageType.Error);
                        return;
                    }

                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                    string backupPath = $"/tmp/{profile.ServiceName}-Backup";
                    string tarFilePath = $"{backupPath}/backup-{timestamp}.tar.gz";

                    // Use sshClient.RunCommand to avoid logging output
                    sshClient.RunCommand($"mkdir -p {backupPath}");

                    // Get total number of files to backup
                    SshCommand totalFilesCommand = sshClient.RunCommand(
                        $"find {profile.RemoteDir} -type f | wc -l"
                    );

                    int totalFiles = int.Parse(totalFilesCommand.Result.Trim());
                    if (totalFiles == 0)
                    {
                        Message.Display("No files to backup.", MessageType.Warning);
                        return;
                    }

                    using ProgressBar progressBar =
                        new(
                            totalFiles,
                            "Creating backup...",
                            new ProgressBarOptions
                            {
                                ForegroundColor = ConsoleColor.Cyan,
                                ForegroundColorDone = ConsoleColor.DarkCyan,
                                BackgroundColor = ConsoleColor.DarkGray,
                                ProgressCharacter = '█'
                            }
                        );

                    // Execute tar command with verbose option and read output to update progress bar
                    using SshCommand command = sshClient.CreateCommand(
                        $"tar -czvf {tarFilePath} -C / {profile.RemoteDir.TrimStart('/')}"
                    );

                    command.CommandTimeout = TimeSpan.FromMinutes(10);
                    var asyncResult = command.BeginExecute();

                    using (var outputReader = new StreamReader(command.OutputStream))
                    {
                        string? line;
                        while (!asyncResult.IsCompleted || !outputReader.EndOfStream)
                        {
                            line = outputReader.ReadLine();
                            if (!string.IsNullOrEmpty(line))
                            {
                                progressBar.Tick($"Adding: {Path.GetFileName(line)}");
                            }
                        }
                    }

                    command.EndExecute(asyncResult);

                    profile.LastBackup = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    ProfileCommands.SaveProfile(profile.Name ?? profileName, profile);

                    Message.Display("Backup created successfully!", MessageType.Success);
                }
                catch (Exception ex)
                {
                    Message.Display($"Error creating backup: {ex.Message}", MessageType.Error);
                }
                finally
                {
                    if (sshClient.IsConnected)
                    {
                        sshClient.Disconnect();
                    }
                }
            }
            catch (Exception ex)
            {
                Message.Display($"Unexpected error: {ex.Message}", MessageType.Error);
            }
        }

        private static void CheckProfileName(
            Action<string> action,
            string profileName,
            string commandName
        )
        {
            if (!string.IsNullOrEmpty(profileName))
            {
                action(profileName);
            }
            else
            {
                Message.Display("Error: Missing profile name.", MessageType.Error);
                Message.Display(
                    $"Usage: customSFTP {commandName} <ProfileName>",
                    MessageType.Warning
                );
            }
        }

        private static void ShowProfile(string profileName)
        {
            ProfileData? profile = ProfileCommands.LoadProfile(profileName);
            if (profile == null || string.IsNullOrEmpty(profileName))
            {
                DisplayProfileError(
                    $"Error: Profile '{profileName}' could not be found.",
                    "show-profile"
                );
                return;
            }
            ProfileCommands.ShowProfile(profile);
        }

        private static void DisplayProfileError(string message, string command)
        {
            Message.Display($"Error: {message}", MessageType.Error);
            Message.Display($"Usage: customSFTP {command} <ProfileName>", MessageType.Warning);
        }
    }
}
