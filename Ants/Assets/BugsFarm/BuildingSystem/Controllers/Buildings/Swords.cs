using System;

namespace BugsFarm.BuildingSystem
{
    public class Swords : BaseTraining
    {
        protected override Type TrainTaskType => typeof(TrainingSwordmanBootstrapTask);
    }
}