namespace CustomSftpTool.Models
{
    public class SshServiceConfig
    {
        public string Host { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string? PrivateKeyPath { get; set; }
    }
}
