using CustomSftpTool.Models;

namespace CustomSftpTool.Interfaces
{
    public interface ISftpServiceFactory
    {
        ISftpService CreateSftpService(ProfileData profile);
    }
}
