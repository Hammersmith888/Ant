using System;
using System.Linq;
using BugsFarm.Services.BootstrapService;
using BugsFarm.SimulationSystem;
using BugsFarm.UserSystem;
using UnityEngine;

namespace BugsFarm.Quest
{
    public class InitQuestsCommand : Command
    {
        private readonly QuestElementDtoStorage _questElementDtoStorage;
        private readonly QuestElementModelStorage _questElementModelStorage;
        private readonly QuestGroupDtoStorage _questGroupDtoStorage;
        private readonly QuestGroupModelStorage _questGroupModelStorage;
        private readonly ISimulationSystem _simulationSystem;
        private readonly IUser _user;


        public InitQuestsCommand(QuestElementDtoStorage questElementDtoStorage,
            QuestElementModelStorage questElementModelStorage,
            QuestGroupModelStorage questGroupModelStorage,
            IUser user,
            QuestGroupDtoStorage questGroupDtoStorage,
            ISimulationSystem simulationSystem)
        {
            _user = user;
            _simulationSystem = simulationSystem;
            _questGroupDtoStorage = questGroupDtoStorage;
            _questGroupModelStorage = questGroupModelStorage;
            _questElementDtoStorage = questElementDtoStorage;
            _questElementModelStorage = questElementModelStorage;
        }

        public override void Do()
        {
              var acceptedQuests = _questElementModelStorage.Get().Where(x => x.Level <= _user.GetLevel()).ToArray();
                    
              var dayQuests = acceptedQuests.OrderBy(x => Guid.NewGuid().ToString())
                  .Take(UnityEngine.Random.Range(3, Mathf.Min(6, acceptedQuests.Length)));
            
            _questElementDtoStorage.Clear();
            foreach (var dayQuest in dayQuests)
            {
                QuestElementDto elementDto = new QuestElementDto()
                {
                    Guid = Guid.NewGuid().ToString(),
                    ModelID = dayQuest.Id,
                    IsStashed = false,
                    CurrentValue = 0,
                    TimeLeftForDiscarding = dayQuest.QuestDurationInMinutes,
                    Level = dayQuest.Level,
                    GoalValue = UnityEngine.Random.Range(dayQuest.MinGoalValue, dayQuest.MaxGoalValue),
                    ReferenceID = dayQuest.ReferenceID
                };
                
                _questElementDtoStorage.Add(elementDto);
            }

            var questGroupModel = _questGroupModelStorage.Get().First();
            var questGroupDto = new QuestGroupDto();
            questGroupDto.ModelID = questGroupModel.ModelID;
            foreach (var chestModel in questGroupModel.VirtualChestModels)
            {
                questGroupDto.VirtualChests.Add(new VirtualChestDto()
                {
                    ModelID = chestModel.ModelID,
                    CurrencyID = chestModel.CurrencyID,
                    Reward = chestModel.Reward,
                    Treshold = chestModel.Treshold,
                    IsOpened = false
                });
            }
            _questGroupDtoStorage.Clear();
            _questGroupDtoStorage.Add(questGroupDto);
            OnDone();
        }
    }
    
}