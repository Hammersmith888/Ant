using System.Linq;
using BugsFarm.Quest;
using BugsFarm.SimulationSystem;
using UnityEngine;
using Zenject;

namespace BugsFarm.SimulatingSystem
{
    public class DailyQuestSimulatingStage
    {
        private readonly QuestElementDtoStorage _questElementDtoStorage;
        private readonly ISimulationSystem _simulationSystem;
        private readonly IInstantiator _instantiator;

        private const float _dayInSeconds = 86400.0f;
        
        public DailyQuestSimulatingStage(IInstantiator instantiator, QuestElementDtoStorage questElementDtoStorage, ISimulationSystem simulationSystem)
        {
            _instantiator = instantiator;
            _simulationSystem = simulationSystem;
            _questElementDtoStorage = questElementDtoStorage;
        }

        public void ReduceDailyQuestTimers(double simulatingTimeInSeconds, double pastGameAge)
        {
            var pastDay = (int) (pastGameAge / _dayInSeconds) + 1;
            var currentDay = (int)((pastGameAge + simulatingTimeInSeconds) / _dayInSeconds) + 1;
            
            foreach (var questElementDto in _questElementDtoStorage.Get())
            {
                if(questElementDto.TimeLeftForDiscarding <= 0.0f)
                    continue;

                questElementDto.TimeLeftForDiscarding = Mathf.Max(0.0f,
                    questElementDto.TimeLeftForDiscarding - (float) (simulatingTimeInSeconds / 60.0));
                if (questElementDto.TimeLeftForDiscarding > 0.0f)
                    continue;

                questElementDto.IsStashed = true;
            }

            if (pastDay != currentDay)
            {
                _instantiator.Instantiate<InitQuestsCommand>().Do();
                var remainder = (pastGameAge + simulatingTimeInSeconds) - _dayInSeconds * (currentDay-1);
                foreach (var questElementDto in _questElementDtoStorage.Get())
                {
                    questElementDto.TimeLeftForDiscarding -= (float)remainder / 60.0f;
                }
            }
        }
    }
}