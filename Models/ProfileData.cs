namespace CustomSftpTool.Models
{
    public class ProfileData
    {
        public string? Name { get; set; }
        public string? Host { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? PrivateKeyPath { get; set; }
        public string? CsprojPath { get; set; }
        public string? LocalDir { get; set; }
        public string? RemoteDir { get; set; }
        public string? ServiceName { get; set; }
        public string? LastDeploy { get; set; }
        public string? LastRestore { get; set; }
        public string? LastBackup { get; set; }
        public List<string> ExcludedFiles { get; set; } = [];
    }
}
