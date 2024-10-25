namespace CustomSftpTool.Interfaces
{
    public interface IDotnetService
    {
        Task<bool> CleanApplicationAsync(string csprojPath);
        Task<bool> PublishApplicationAsync(string csprojPath, string outputPath);
    }
}
