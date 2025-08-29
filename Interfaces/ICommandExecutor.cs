namespace CustomSftpTool.Interfaces;

public interface ICommandExecutor
{
    Task Execute(string[] args);
}