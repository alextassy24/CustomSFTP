namespace CustomSftpTool.Interfaces
{
    public interface ICommandParser
    {
        ICommand? Parse(string[] args);
    }
}
