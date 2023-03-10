using BugsFarm.Services.BootstrapService;
using BugsFarm.Services.StatsService;
using BugsFarm.Utility;
using Zenject;

namespace BugsFarm.UserSystem
{
    public class InitUserCommand : Command
    {
        private readonly IInstantiator _instantiator;
        private readonly IUser _user;

        public InitUserCommand(IInstantiator instantiator, 
                               IUser user)
        {
            _instantiator = instantiator;
            _user = user;
        }
        public override void Do()
        {
            var userStatModel = ConfigHelper.Load<UserStatModel>("UserStatModel")[0];
            _instantiator.Instantiate<CreateStatsCollectionCommand<StatsCollection>>()
                .Execute(new CreateStatsCollectionProtocol(_user.Id, userStatModel.Stats));
            _user.Initialize();
            OnDone();
        }
    }
}