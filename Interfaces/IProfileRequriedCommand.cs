using CustomSftpTool.Commands;

namespace CustomSftpTool.Interfaces
{
    public interface IProfileRequiredCommand : ICommand
    {
        string ProfileName { get; }
        void SetServices(
            IDeployService deployService,
            ISshService sshService,
            ISftpService sftpService
        );
    }
}
