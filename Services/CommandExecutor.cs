using CustomSftpTool.Interfaces;
using CustomSftpTool.Parsers;

namespace CustomSftpTool.Services
{
    public class CommandExecutor(ICommandParser parser, ILoggerService logger) : ICommandExecutor
    {
        private readonly ICommandParser _parser = parser;
        private readonly ILoggerService _logger = logger;

        public async Task Execute(string[] args)
        {
            var command = _parser.Parse(args);
            if (command != null)
            {
                await command.Execute();
            }
            else
            {
                _logger.LogWarning("Command not recognized.");
            }
        }
    }
}
