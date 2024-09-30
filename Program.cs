using CustomSftpTool.Commands;
using CustomSftpTool.Data;
using CustomSftpTool.Models;
using CustomSftpTool.Profile;
using Renci.SshNet;
using Serilog;

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

                string command = args[0].ToLower();
                string profileName = string.Empty;

                // Combine all arguments after the command into a single profile name
                if (args.Length > 1)
                {
                    profileName = string.Join(' ', args.Skip(1));
                }

                switch (command)
                {
                    case "--deploy":
                        if (!string.IsNullOrEmpty(profileName))
                        {
                            await ProfileCommands.DeployUsingProfile(profileName);
                        }
                        else
                        {
                            Message.Display("Error: Missing profile name.", MessageType.Error);
                            Message.Display(
                                "Usage: customSFTP --deploy <ProfileName>",
                                MessageType.Warning
                            );
                        }

                        break;
                    case "--add-profile":
                        ProfileCommands.AddProfile();
                        break;
                    case "--list-profiles":
                        ProfileCommands.ListProfiles();
                        break;
                    case "--show-profile":
                        CheckProfileName(ShowProfile, profileName, "--show-profile");
                        break;
                    case "--edit-profile":
                        CheckProfileName(
                            ProfileCommands.EditProfile,
                            profileName,
                            "--edit-profile"
                        );
                        break;
                    case "--remove-profile":
                        CheckProfileName(
                            ProfileCommands.RemoveProfile,
                            profileName,
                            "--remove-profile"
                        );
                        break;
                    case "--stop-service":
                        ExecuteServiceCommand(profileName, "stop");
                        break;
                    case "--start-service":
                        ExecuteServiceCommand(profileName, "start");
                        break;
                    case "--restart-service":
                        ExecuteServiceCommand(profileName, "restart");
                        break;
                    case "--check-service":
                        ExecuteServiceCommand(profileName, "is-active");
                        break;
                    case "--help":
                        ShowHelp();
                        break;
                    default:
                        Console.WriteLine();
                        Message.Display($"Unknown command: {command}", MessageType.Warning);
                        ShowHelp();
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "An unhandled exception occurred.");
                Message.Display($"An unexpected error occurred: {ex.Message}", MessageType.Error);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static void ShowHelp()
        {
            Message.Display("Description:", MessageType.Warning);
            Console.WriteLine("  Custom SFTP Deployment Tool");

            Message.Display("\nUsage:", MessageType.Warning);
            Console.WriteLine("  customSFTP [command] [options]");

            Message.Display("\nCommands:", MessageType.Warning);
            PrintOption("--deploy <ProfileName>", "Deploy using a profile");
            PrintOption("--add-profile", "Add a new profile");
            PrintOption("--list-profiles", "List all available profiles");
            PrintOption("--show-profile <ProfileName>", "Show details of a profile");
            PrintOption("--edit-profile <ProfileName>", "Edit an existing profile");
            PrintOption("--remove-profile <ProfileName>", "Remove a profile");
            PrintOption("--help", "Show help and usage information");

            Message.Display(
                "\nOptions (for use with commands like --add-profile or --edit-profile):",
                MessageType.Warning
            );
            PrintOption("--host <host>", "The SSH host");
            PrintOption("--username <username>", "The SSH username");
            PrintOption("--privateKeyPath <path>", "Path to the private SSH key");
            PrintOption("--csprojPath <path>", "Path to the .csproj file");
            PrintOption("--localDir <path>", "Local directory for the build output");
            PrintOption("--remoteDir <path>", "Remote directory to deploy to");
            PrintOption("--serviceName <name>", "Name of the service to restart");
        }

        private static void PrintOption(string option, string description)
        {
            Console.WriteLine($"  {option, -35} {description}");
        }

        private static void ExecuteServiceCommand(string profileName, string action)
        {
            if (!string.IsNullOrEmpty(profileName))
            {
                var profile = ProfileCommands.LoadProfile(profileName);
                if (profile != null)
                {
                    ExecuteSystemCtlCommand(profile, action);
                }
                else
                {
                    Message.Display(
                        $"Error: Profile '{profileName}' could not be found.",
                        MessageType.Error
                    );
                }
            }
            else
            {
                Message.Display("Error: Missing profile name.", MessageType.Error);
                Message.Display(
                    $"Usage: customSFTP --{action}-service <ProfileName>",
                    MessageType.Warning
                );
            }
        }

        private static void ExecuteSystemCtlCommand(ProfileData profile, string action)
        {
            using SshClient sshClient = SshCommands.CreateSshClient(
                profile.Host,
                profile.UserName,
                profile.PrivateKeyPath
            );

            sshClient.Connect();
            SshCommands.ExecuteCommand(sshClient, $"sudo systemctl {action} {profile.ServiceName}");
            sshClient.Disconnect();
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
            if (profile != null)
            {
                ProfileCommands.ShowProfile(profile);
            }
            else
            {
                Message.Display(
                    $"Error: Profile '{profileName}' could not be found.",
                    MessageType.Error
                );
            }
        }
    }
}
