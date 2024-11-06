namespace CustomSftpTool.Commands.Implementations
{
    public class HelpCommand : ICommand
    {
        public Task Execute()
        {
            Console.WriteLine(
                "Available commands: deploy, add-profile, edit-profile, list-profiles, remove-profile, show-profile"
            );
            return Task.CompletedTask;
        }
    }
}
