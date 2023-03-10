using System;

namespace BugsFarm.BuildingSystem
{
    public class OutdoorPear : BaseTraining
    {
        protected override Type TrainTaskType => typeof(TrainingHeavySquadBootstrapTask);
    }
}