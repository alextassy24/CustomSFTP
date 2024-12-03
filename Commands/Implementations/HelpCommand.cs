namespace CustomSftpTool.Commands.Implementations
{
    public class HelpCommand : ICommand
    {
        public Task Execute()
        {
            Console.WriteLine("Custom Deployment Tool");
            Console.WriteLine("Usage: customSFTP [command] [options]");
            Console.WriteLine("Commands:");
            Console.WriteLine("  help                          - Show help information");
            Console.WriteLine("  add-profile                   - Add a new profile");
            Console.WriteLine("  list-profiles                 - List all available profiles");
            Console.WriteLine("  deploy <ProfileName>          - Deploy using a profile");
            Console.WriteLine("  show-profile <ProfileName>    - Show details of a profile");
            Console.WriteLine("  edit-profile <ProfileName>    - Edit an existing profile");
            Console.WriteLine("  remove-profile <ProfileName>  - Remove a profile");
            return Task.CompletedTask;
        }
    }
}
