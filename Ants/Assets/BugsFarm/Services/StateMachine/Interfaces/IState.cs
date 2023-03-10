using BugsFarm.Services.StorageService;

namespace BugsFarm.Services.StateMachine
{
    public interface IState : IStorageItem
    {
        void OnEnter(params object[] args);
        void OnExit();
    }
}