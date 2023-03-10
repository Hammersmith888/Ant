using System;
using BugsFarm.AnimationsSystem;
using Random = UnityEngine.Random;

namespace BugsFarm.UnitSystem.Obsolete
{
    [Serializable]
    public class SM_TrainSwordman : SM_TrainPikeman
    {
        protected override ObjType TrainObjectType => ObjType.str_Swords;

        public SM_TrainSwordman(int priority, StateAnt aiType) : base(priority, aiType) { }

        protected override AnimKey RandomTrain()
        {
            var randomAnim = Random.Range(0, 2);
            switch (randomAnim)
            {
                case 0: TrainCounter = 10; 
                    return AnimKey.Attack;

                case 1: TrainCounter = 8; 
                    return AnimKey.Training;

                default: return AnimKey.None;
            }
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