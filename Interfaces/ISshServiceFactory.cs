using CustomSftpTool.Models;

namespace CustomSftpTool.Interfaces;

public interface ISshServiceFactory
{
    ISshService CreateSshService(ProfileData profile);
}