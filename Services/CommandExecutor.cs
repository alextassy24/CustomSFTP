using CustomSftpTool.Interfaces;

namespace CustomSftpTool.Services;

public class CommandExecutor(ICommandParser parser, ILoggerService logger) : ICommandExecutor
{
    public async Task Execute(string[] args)
    {
        var command = parser.Parse(args);
        if (command != null)
        {
            await command.Execute();
        }
        else
        {
            logger.LogWarning("Command not recognized.");
        }
    }
}