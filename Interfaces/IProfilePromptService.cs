using CustomSftpTool.Models;

namespace CustomSftpTool.Interfaces;

public interface IProfilePromptService
{
    ProfileData PromptForProfileData();
}