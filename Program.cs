using CustomSftpTool.Models;
using CustomSftpTool.Profile;
using Serilog;

namespace CustomSftpTool
{
    internal class Program
    {
        // // Connection info
        // private static readonly string Host = @"193.230.3.37";
        // private static readonly string UserName = "iciadmin";
        // private static readonly string PrivateKeyPath = @"C:\Users\admin\.ssh\id_rsa"; // Updated path to OpenSSH key

        // // Define your local and remote directories
        // private static readonly string CsprojPath = @"C:\Users\admin\Desktop\CdM\CdM\CdM.csproj";
        // private static readonly string LocalDir = @"C:\Users\admin\Desktop\CdMDeploy";
        // private static readonly string RemoteDir = "/var/www/case";
        // private static readonly string ServiceName = "case";

        // Define exclusions relative to the local directory
        // private static readonly List<string> Exclusions =
        // [
        //     "appsettings.json",
        //     "appsettings.Development.json",
        //     "wwwroot\\Files"
        // ];

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
                            Console.WriteLine("Error: Missing profile name.");
                            Console.WriteLine("Usage: customSSH --deploy <ProfileName>");
                        }
                        break;
                    case "--add-profile":
                        ProfileCommands.AddProfile();
                        break;
                    case "--list-profiles":
                        ProfileCommands.ListProfiles();
                        break;
                    case "--show-profile":
                        if (!string.IsNullOrEmpty(profileName))
                        {
                            ProfileData? profile = ProfileCommands.LoadProfile(profileName);
                            if (profile != null)
                                ProfileCommands.ShowProfile(profile);
                            else
                            {
                                Console.WriteLine(
                                    $"Error: Profile '{profileName}' could not be found."
                                );
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error: Missing profile name.");
                            Console.WriteLine("Usage: customSSH --show-profile <ProfileName>");
                        }
                        break;
                    case "--edit-profile":
                        if (!string.IsNullOrEmpty(profileName))
                        {
                            ProfileCommands.EditProfile(profileName);
                        }
                        else
                        {
                            Console.WriteLine("Error: Missing profile name.");
                            Console.WriteLine("Usage: customSSH --edit-profile <ProfileName>");
                        }
                        break;
                    case "--remove-profile":
                        if (!string.IsNullOrEmpty(profileName))
                        {
                            ProfileCommands.RemoveProfile(profileName);
                        }
                        else
                        {
                            Console.WriteLine("Error: Missing profile name.");
                            Console.WriteLine("Usage: customSSH --remove-profile <ProfileName>");
                        }
                        break;
                    case "--help":
                        ShowHelp();
                        break;
                    default:
                        Console.WriteLine($"Unknown command: {command}");
                        ShowHelp();
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "An unhandled exception occurred.");
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static void ShowHelp()
        {
            Console.WriteLine("Description:");
            Console.WriteLine("  Custom SFTP Deployment Tool");

            Console.WriteLine("\nUsage:");
            Console.WriteLine("  customSSH [command] [options]");

            Console.WriteLine("\nCommands:");
            PrintOption("--deploy <ProfileName>", "Deploy using a profile");
            PrintOption("--add-profile", "Add a new profile");
            PrintOption("--list-profiles", "List all available profiles");
            PrintOption("--show-profile <ProfileName>", "Show details of a profile");
            PrintOption("--edit-profile <ProfileName>", "Edit an existing profile");
            PrintOption("--remove-profile <ProfileName>", "Remove a profile");
            PrintOption("--help", "Show help and usage information");

            Console.WriteLine(
                "\nOptions (for use with commands like --add-profile or --edit-profile):"
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
    }
}
