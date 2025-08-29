using CustomSftpTool.Interfaces;
using CustomSftpTool.Models;

namespace CustomSftpTool.Services;

public class SshServiceFactory : ISshServiceFactory
{
    public ISshService CreateSshService(ProfileData profile)
    {
        return new SshService(
            profile.Host!,
            profile.UserName!,
            profile.Password,
            profile.PrivateKeyPath
        );
    }
}