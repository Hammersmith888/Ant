using System;
using BugsFarm.AnimationsSystem;
using Random = UnityEngine.Random;

namespace BugsFarm.UnitSystem.Obsolete
{
    [Serializable]
    public class SM_TrainHeavySquad : SM_TrainPikeman
    {
        protected override ObjType TrainObjectType => ObjType.str_OutDoorPear;

        public SM_TrainHeavySquad(int priority, StateAnt aiType) : base(priority, aiType) { }

        protected override AnimKey RandomTrain()
        {
            TrainCounter = Random.Range(6, 13); 
            return AnimKey.Attack;
        }
        protected override void SetSound(){}
        protected override void FreeSound(){}
        protected override void SetCycle()
        {
            SoundCycle++;
            CurrentTrain = RandomTrain();
            RestCounter = 5;
        }
    }
}