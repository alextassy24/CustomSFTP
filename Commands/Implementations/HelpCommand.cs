using CustomSftpTool.Interfaces;

namespace CustomSftpTool.Commands.Implementations
{
    public class HelpCommand : ICommand
    {
        public Task Execute()
        {
            Console.WriteLine("Custom SFTP Tool - Command Reference");
            Console.WriteLine();
            Console.WriteLine("Usage: customSFTP [command] [options]");
            Console.WriteLine();
            Console.WriteLine("Available Commands:");
            Console.WriteLine();
            Console.WriteLine("General Commands:");
            Console.WriteLine("  help                       - Show this help message.");
            Console.WriteLine("  add-profile                - Add a new profile.");
            Console.WriteLine("  list-profiles              - List all available profiles.");
            Console.WriteLine("  show-profile <ProfileName> - Display details of a specific profile.");
            Console.WriteLine("  edit-profile <ProfileName> - Edit an existing profile.");
            Console.WriteLine("  remove-profile <ProfileName> - Remove a profile.");
            Console.WriteLine();
            Console.WriteLine("Deployment Commands:");
            Console.WriteLine("  deploy <ProfileName> [--force] - Deploy using the specified profile.");
            Console.WriteLine("  backup <ProfileName>         - Backup files for the specified profile.");
            Console.WriteLine();
            Console.WriteLine("SSH Commands:");
            Console.WriteLine("  status-service <ProfileName> - Check the status of a service.");
            Console.WriteLine("  check-service <ProfileName>  - Verify if a service is active.");
            Console.WriteLine("  stop-service <ProfileName>   - Stop a running service.");
            Console.WriteLine("  start-service <ProfileName>  - Start a service.");
            Console.WriteLine("  restart-service <ProfileName> - Restart a service.");
            Console.WriteLine();
            return Task.CompletedTask;
        }
    }
}
