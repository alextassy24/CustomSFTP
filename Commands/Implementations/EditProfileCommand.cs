using CustomSftpTool.Data;
using CustomSftpTool.Interfaces;

namespace CustomSftpTool.Commands.Implementations
{
    public class EditProfileCommand(
        string profileName,
        List<string> fields,
        Dictionary<string, string> fieldSets,
        IProfileService profileService
    ) : ICommand
    {
        private readonly string _profileName = profileName;
        private readonly List<string> _fields = fields;
        private readonly Dictionary<string, string> _fieldSets = fieldSets;
        private readonly IProfileService _profileService = profileService;

        public Task Execute()
        {
            var profile = _profileService.LoadProfile(_profileName);
            if (profile == null)
            {
                Message.Display($"Error: Profile '{_profileName}' could not be found.", MessageType.Error);
                return Task.CompletedTask;
            }

            if (_fieldSets.Count > 0)
            {
                _profileService.UpdateProfileFields(profile, _fieldSets);
            }
            else if (_fields.Count > 0)
            {
                _profileService.PromptToUpdateProfile(profile);
            }

            _profileService.RenameProfileFile(_profileName, profile.Name!);
            _profileService.SaveProfile(profile);

            Message.Display($"Profile '{profile.Name}' updated successfully.", MessageType.Success);
            return Task.CompletedTask;
        }
    }
}
