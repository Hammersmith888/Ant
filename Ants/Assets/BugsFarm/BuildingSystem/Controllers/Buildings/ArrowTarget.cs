using System;

namespace BugsFarm.BuildingSystem
{
    public class ArrowTarget : BaseTraining
    {
        protected override Type TrainTaskType => typeof(TrainingArcherBootstrapTask);
    }
}