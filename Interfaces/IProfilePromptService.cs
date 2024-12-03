using CustomSftpTool.Models;

namespace CustomSftpTool.Interfaces
{
    public interface IProfilePromptService
    {
        ProfileData PromptForProfileData();
        List<string> PromptForExcludedFiles(List<string>? existingExclusions = null);
    }
}
