using BugsFarm.Services.StateMachine;
using BugsFarm.UserSystem;
using Zenject;

namespace BugsFarm.Quest
{
    public class CoinsCollectQuestElement : QuestElement
    {
        private readonly IUser _user;

        public CoinsCollectQuestElement(string guid,
                                        QuestElementModelStorage questElementModelStorage,
                                        QuestElementDtoStorage questElementDtoStorage,
                                        IInstantiator instantiator,
                                        IUser user) : base(guid, 
                                                            questElementModelStorage, 
                                                            questElementDtoStorage, 
                                                            instantiator)
        {
            _user = user;
        }

        public override void Initialize()
        {
            base.Initialize();
            CheckUserLevel();
        }

        private void CheckUserLevel()
        {
            int level = _user.GetLevel();
            int totalCoinsToCollect = (level - 1) * 50 + 25;
            //int totalCoinsToCollect = 0;
            _questDto.GoalValue = totalCoinsToCollect;
        }
    }
}