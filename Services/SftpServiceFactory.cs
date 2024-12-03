using CustomSftpTool.Interfaces;
using CustomSftpTool.Models;
using Renci.SshNet;

namespace CustomSftpTool.Services
{
    public class SftpServiceFactory : ISftpServiceFactory
    {
        public ISftpService CreateSftpService(ProfileData profile)
        {
            var connectionInfo = new ConnectionInfo(
                profile.Host!,
                profile.UserName!,
                !string.IsNullOrEmpty(profile.Password)
                    ? new PasswordAuthenticationMethod(profile.UserName!, profile.Password!)
                    : new PrivateKeyAuthenticationMethod(profile.UserName!, new PrivateKeyFile(profile.PrivateKeyPath!))
            );

            var sftpClient = new SftpClient(connectionInfo);
            return new SftpService(sftpClient);
        }
    }
}
