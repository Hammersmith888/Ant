using BugsFarm.Services.CommandService;

namespace BugsFarm.UserSystem
{
    public readonly struct UserLevelChangedProtocol : IProtocol
    {
        public readonly int Level;
        public UserLevelChangedProtocol(int level)
        {
            Level = level;
        }
    }
}