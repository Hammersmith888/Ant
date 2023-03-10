using BugsFarm.Services.StatsService;
using BugsFarm.TaskSystem;
using UnityEngine;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public class LarvaGrowthTask : BaseTask
    {
        private const string _growthTimeStatKey = "stat_growthTime";
        private const string _growthStageStatKey = "stat_stage";
        protected StatsCollectionStorage StatsCollectionStorage;
        protected IInstantiator Instantiator;
        protected StatsCollection _statsCollection;

        protected int StageCount => (int) GrowthStageStat.CurrentValue;
        protected StatVital GrowthTimeStat;
        protected StatVital GrowthStageStat;
        protected ITask GrowthTask;
        protected string UnitId;

        [Inject]
        private void Inject(StatsCollectionStorage statCollectionStorage,
                            IInstantiator instantiator)
        {
            StatsCollectionStorage = statCollectionStorage;
            Instantiator = instantiator;
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned) return;
            base.Execute(args);
            UnitId = (string) args[0];
            _statsCollection = StatsCollectionStorage.Get(UnitId);
            GrowthTimeStat = _statsCollection.Get<StatVital>(_growthTimeStatKey);
            GrowthStageStat = _statsCollection.Get<StatVital>(_growthStageStatKey);

            UpdateStage();
            InitGrowthTask();
        }

        protected virtual void InitGrowthTask()
        {
            if (Finalized || GrowthTask != null) return;

            var growthTask = Instantiator.Instantiate<TimerFromStatKeyTask>(new object[]{TimeType.Minutes});
            growthTask.OnComplete += _ => OnGrowthStageEnd();
            growthTask.SetUpdateAction(UpdateGrowthTime);
            GrowthTask = growthTask;
            GrowthTask.Execute(UnitId, _growthTimeStatKey);
        }

        private void UpdateGrowthTime(float timeLeft)
        {
            GrowthTimeStat.CurrentValue = Mathf.Max(0, timeLeft);
        }
        protected virtual void OnGrowthStageEnd()
        {
            if (Finalized) return;
            GrowthStageStat.CurrentValue -= 1;
            GrowthTask = null;
            if (StageCount == 0)
            {
                Completed();
                return;
            }

            GrowthTimeStat.SetMax();
            UpdateStage();
            InitGrowthTask();
        }

        protected virtual void UpdateStage()
        {
            if (Finalized) return;
            var protocol = new UnitSetStageProtocol(UnitId, StageCount - 1);
            Instantiator.Instantiate<UnitSetStageCommand>().Execute(protocol);
        }

        protected override void OnDisposed()
        {
            GrowthTask?.Interrupt();
            GrowthTimeStat = null;
            GrowthStageStat = null;
            GrowthTask = null;
            UnitId = null;
            StatsCollectionStorage = null;
            Instantiator = null;
            base.OnDisposed();
        }
    }
}