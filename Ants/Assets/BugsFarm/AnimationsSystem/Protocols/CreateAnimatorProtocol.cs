using System;
using BugsFarm.Services.CommandService;

namespace BugsFarm.AnimationsSystem
{
    public readonly struct CreateAnimatorProtocol : IProtocol
    {
        public string Guid { get; }
        public string AnimModelId { get; }
        public object[] Args { get; }
        public Action<ISpineAnimator> Result { get; }

        public CreateAnimatorProtocol(string guid, string animModelId, params object[] args)
        {
            Guid = guid;
            AnimModelId = animModelId;
            Args = args ?? new object[0];
            Result = default;
        }

        public CreateAnimatorProtocol(string guid, string animModelId, Action<ISpineAnimator> result, params object[] args)
        {
            Guid = guid;
            AnimModelId = animModelId;
            Args = args ?? new object[0];
            Result = result;
        }
    }
}