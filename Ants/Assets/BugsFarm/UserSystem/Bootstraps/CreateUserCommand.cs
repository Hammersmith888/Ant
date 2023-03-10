using BugsFarm.Services.BootstrapService;
using BugsFarm.Services.SaveManagerService;
using BugsFarm.Utility;
using UnityEngine;

namespace BugsFarm.UserSystem
{
    public class CreateUserCommand : Command
    {
        private readonly string _userId;
        private readonly IUserInternal _userController;
        private readonly ISaveManager _saveManager;

        public CreateUserCommand(string userId,
                                 IUserInternal userController,
                                 ISaveManager saveManager)
        {
            _userId = userId;
            _userController = userController;
            _saveManager = saveManager;
        }

        public override void Do()
        {

            var userPath = PathConstants.GetUserPath(_userId);
            var dto = _saveManager.HasSaves(userPath) ? 
                JsonUtility.FromJson<UserDto>(_saveManager.Load(userPath)) : 
                new UserDto(_userId);
            
            _userController.InitializeInternal(dto);
            _saveManager.Save(JsonUtility.ToJson(dto), userPath);
            OnDone();
        }
    }
}