namespace CustomSftpTool.Interfaces
{
    public interface ISshService
    {
        void Connect();
        void Disconnect();
        string? ExecuteCommand(string command);
    }
}
