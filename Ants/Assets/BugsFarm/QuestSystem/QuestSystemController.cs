using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.SimulationSystem;
using BugsFarm.TaskSystem;
using BugsFarm.UserSystem;
using UniRx;
using UnityEngine;
using Zenject;

namespace BugsFarm.Quest
{
    public class QuestSystemController
    {
        public IReadOnlyDictionary<string, List<QuestElement>> DailyQuests => _dailyQuestsCollection;
        public int CurrentDay => _currentDay;

        public event Action OnQuestsRefreshed;
        public event Action<string> QuestExpiredEvent; 

        private readonly IInstantiator _instantiator;
        private readonly ISimulationSystem _simulationSystem;

        private readonly QuestGroupDtoStorage _questGroupDtoStorage;
        private readonly QuestGroupModelStorage _questGroupModelStorage;
        private readonly QuestElementDtoStorage _questElementDtoStorage;
        private readonly QuestElementModelStorage _questElementModelStorage;

        private Dictionary<string, List<QuestElement>> _dailyQuestsCollection;

        private SimulatedTimerTask _questGroupTimer;
        private IDisposable _questUpdateEvent;

        private int _currentDay;
        private float _timeLeftInSeconds;
        private readonly IUser _user;

        private const double _dayInSeconds = 86400.0; 

        public QuestSystemController(ISimulationSystem simulationSystem,
            IUser user,
            QuestGroupModelStorage questGroupModelStorage,
            QuestElementModelStorage questElementModelStorage,
            QuestElementDtoStorage questElementDtoStorage,
            QuestGroupDtoStorage questGroupDtoStorage,
            IInstantiator instantiator)
        {
            _user = user;
            _instantiator = instantiator;
            _questGroupDtoStorage = questGroupDtoStorage;
            _questGroupModelStorage = questGroupModelStorage;
            _questElementModelStorage = questElementModelStorage;
            _questElementDtoStorage = questElementDtoStorage;
            _simulationSystem = simulationSystem;
            _dailyQuestsCollection = new Dictionary<string, List<QuestElement>>();
        }

        public void Initialize()
        {
            _questUpdateEvent = MessageBroker.Default.Receive<QuestUpdateProtocol>().Subscribe(UpdateQuests);
            UpdateTime();
            CreateTimer();
            LoadDailyQuests();
        }

        private void UpdateQuests(QuestUpdateProtocol protocol)
        {
            if (!_dailyQuestsCollection.ContainsKey(protocol.QuestType))
                return;

            foreach (var dailyQuest in _dailyQuestsCollection[protocol.QuestType])
            {
                var dto = _questElementDtoStorage.Get(dailyQuest.Guid);
                if(dto.ReferenceID == protocol.ReferenceID)
                    dailyQuest.AddProgress(protocol.Value);
            }
        }
        public void Dispose()
        {
            _questUpdateEvent.Dispose();
        }
        private void UpdateTime()
        {
            _currentDay = (int)(_simulationSystem.GameAge / _dayInSeconds) + 1;
            _timeLeftInSeconds = (float)(_dayInSeconds - (_simulationSystem.GameAge % _dayInSeconds));
        }
        
        public void RefreshQuestList(ITask timerTask)
        {
            _questGroupTimer.OnComplete -= RefreshQuestList;
            DisposeQuests();
            UpdateTime();
            CreateTimer();
            _instantiator.Instantiate<InitQuestsCommand>().Do();
            LoadDailyQuests();
            CreateNewQuestGroup();
            OnQuestsRefreshed?.Invoke();
        }
        
        private void CreateNewQuestGroup()
        {
            var questGroupModel = _questGroupModelStorage.Get(_currentDay.ToString());
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
            _questGroupDtoStorage.Add(questGroupDto);
        }

        private void CreateTimer()
        {
            _questGroupTimer = _instantiator.Instantiate<SimulatedTimerTask>(new object[]{TimeType.Minutes});
            _questGroupTimer.Execute(_timeLeftInSeconds);
            _questGroupTimer.OnComplete += RefreshQuestList;
        }

        private void LoadDailyQuests()
        {
            /*var acceptedQuests = _questElementDtoStorage.Get().Where(x => x.Level <= _user.GetLevel()).ToArray();
                
            var dayQuests = acceptedQuests.OrderBy(x => Guid.NewGuid().ToString())
                                                                        .Take(UnityEngine.Random.Range(3, Mathf.Min(6, acceptedQuests.Length)));*/
            
            foreach (var dayQuest in _questElementDtoStorage.Get())
            {
                var questModel = _questElementModelStorage.Get(dayQuest.ModelID);
                
                if(!_dailyQuestsCollection.ContainsKey(questModel.QuestType))
                    _dailyQuestsCollection.Add(questModel.QuestType, new List<QuestElement>());

                var questElement = GetQuestElement(dayQuest.Guid, dayQuest.ModelID);
                _dailyQuestsCollection[questModel.QuestType].Add(questElement);
                questElement.Initialize();
                questElement.OnQuestExpired += OnQuestExpired;
            }
        }

        private void OnQuestExpired(string guid)
        {
            QuestExpiredEvent?.Invoke(guid);
        }

        private void DisposeQuests()
        {
            foreach (var questType in _dailyQuestsCollection.Values)
            {
                for (int i = 0; i < questType.Count; i++)
                {
                    questType[i].OnQuestExpired -= OnQuestExpired;
                    questType[i].Dispose();
                }
            }
            
            _dailyQuestsCollection.Clear();
        }

        private QuestElement GetQuestElement(string guid, string modelID)
        {
            switch (modelID)
            {
                case "0": return _instantiator.Instantiate<CoinsCollectQuestElement>(new object[]{guid});
                case "9": return _instantiator.Instantiate<WinWithQuestElement>(new object[]{guid}); 
                default: return _instantiator.Instantiate<QuestElement>(new object[]{guid});
            };
        }
    /* private void SetNewDayQuests()
       {
           var dayQuests = _questElementModelStorage.Get().Where(x => x.Day == _currentDay);
           
           foreach (var dayQuest in dayQuests)
           {
               QuestElementDto elementDto = new QuestElementDto()
               {
                   Guid = Guid.NewGuid().ToString(),
                   ModelID = dayQuest.Id,
                   IsStashed = false,
                   CurrentValue = 0,
                   TimeLeftForDiscarding = dayQuest.QuestDurationInMinutes,
                   GoalValue = UnityEngine.Random.Range(dayQuest.MinGoalValue, dayQuest.MaxGoalValue),
                   ReferenceID = dayQuest.ReferenceID
               };
               
               _questElementDtoStorage.Add(elementDto);
           }
       }*/   
    }
}