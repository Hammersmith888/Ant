using System;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using BugsFarm.TaskSystem;
using BugsFarm.UserSystem;
using UniRx;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class Safe : ISceneEntity, IInitializable
    {
        public string Id { get; }
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly IInstantiator _instantiator;

        private const string _resourceStatKey = "stat_resource";
        private const string _bossRewardStatKey = "stat_bossReward";
        private const string _levelRewardStatKey = "stat_levelReward";
        private const string _questRewardStatKey = "stat_questReward";
        private const string _arenaRewardStatKey = "stat_arenaReward";
        private const string _seasonRewardStatKey = "stat_seasonReward";
        private const string _takeTimeStatKey = "stat_takeTime";

        private IDisposable _achievementEvent;
        private StatsCollection _statsCollection;
        private StatVital _takeTimeStat;
        private StatVital _resourceStat;
        private ITask _takeTimer;

        public Safe(string guid,
                    StatsCollectionStorage statsCollectionStorage,
                    IInstantiator instantiator)
        {
            _statsCollectionStorage = statsCollectionStorage;
            _instantiator = instantiator;
            Id = guid;
        }

        public void Initialize()
        {
            _statsCollection = _statsCollectionStorage.Get(Id);
            _takeTimeStat = _statsCollection.Get<StatVital>(_takeTimeStatKey);
            _resourceStat = _statsCollection.Get<StatVital>(_resourceStatKey);
            _achievementEvent = MessageBroker.Default
                .Receive<AchievementProtocol>()
                .Subscribe(OnAchivementEventHandler);

            if (IsFull())
            {
                InitTakeTimer();
            }
        }

        public void Dispose()
        {
            _achievementEvent?.Dispose();
            _achievementEvent = null;
            _takeTimeStat = null;
            _resourceStat = null;
        }

        private bool IsFull()
        {
            return _resourceStat.CurrentValue >= _resourceStat.Value;
        }

        private void InitTakeTimer()
        {
            _takeTimer?.Interrupt();
            _takeTimer = _instantiator.Instantiate<TimerFromStatKeyTask>(new object[]{TimeType.Minutes});
            _takeTimer.OnComplete += _ =>
            {
                _takeTimeStat.SetMax();
                _resourceStat.CurrentValue = 0;
            };
            _takeTimer.Execute(Id,_takeTimeStatKey);
        }

        private void OnAchivementEventHandler(AchievementProtocol protocol)
        {
            if (IsFull())
            {
                return;
            }
            var valueAdd = 0f;
            switch (protocol.Id)
            {
                case Achievement.FightedBoss:
                    valueAdd += _statsCollection.GetValue(_bossRewardStatKey);
                    break;
                case Achievement.LevelUp:
                    valueAdd += _statsCollection.GetValue(_levelRewardStatKey);
                    break;
                case Achievement.QuestDone:
                    valueAdd += _statsCollection.GetValue(_questRewardStatKey);
                    break;
                case Achievement.ArenaWin:
                    valueAdd += _statsCollection.GetValue(_arenaRewardStatKey);
                    break;
                case Achievement.NewSeason:
                    valueAdd += _statsCollection.GetValue(_seasonRewardStatKey);
                    break;
            }

            if (valueAdd > 0)
            {
                _resourceStat.CurrentValue += valueAdd;
                if (IsFull())
                {
                    _takeTimeStat.SetMax();
                    InitTakeTimer();
                }
            }
        }
    }
}