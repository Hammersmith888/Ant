using System;

namespace BugsFarm.BuildingSystem
{
    public class Pikes : BaseTraining
    {
        protected override Type TrainTaskType => typeof(TrainingPikemanBootstrapTask);
    }
}