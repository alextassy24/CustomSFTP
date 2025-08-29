using CustomSftpTool.Models;

namespace CustomSftpTool.Interfaces;

public interface IDeployService
{
    Task<bool> RunDeploymentAsync(ProfileData profile, bool force);
}