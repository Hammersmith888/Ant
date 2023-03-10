using BugsFarm.Services.CommandService;

namespace BugsFarm.AnimationsSystem
{
    public readonly struct RemoveAnimatorProtocol : IProtocol
    {
        public readonly string Guid;

        public RemoveAnimatorProtocol(string guid)
        {
            Guid = guid;
        }
    }
}