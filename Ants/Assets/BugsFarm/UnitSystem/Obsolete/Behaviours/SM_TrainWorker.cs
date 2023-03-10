using System;
using BugsFarm.AnimationsSystem;
using Random = UnityEngine.Random;

namespace BugsFarm.UnitSystem.Obsolete
{
    [Serializable]
    public class SM_TrainWorker : SM_TrainPikeman
    {
        protected override ObjType TrainObjectType => ObjType.str_Pikes;
        public SM_TrainWorker(int priority, StateAnt aiType) : base(priority, aiType) { }
        public override void PostLoadRestore()
        {
            if(State == TrainState.GoTrain)
            {
                AiMover.OnComplete += OnDestenationComplete;
            }
            SetAnim();
        }
        protected override void OnTrain()
        {
            if (TrainCounter <= 0)
            {
                Transition(TrainState.Rest);
                return;
            }
            TrainCounter--;
            TimerXp.Unpause();
            var point = Equipment.GetPoint(AntPresenter);
            AiMover.SetLook(point.LookLeft);
            SetAnim();
        }
        protected override void OnRest()
        {
            if (RestCounter <= 0)
            {
                SetCycle();
                Transition(TrainState.Train);
                return;
            }
            RestCounter--;
            TimerXp.Pause();
            SetAnim();
        }
        protected override void OnNone()
        {
            base.OnNone();
            TrainCounter = RestCounter = 0;
        }
        protected override AnimKey RandomTrain()
        { 
            var randomAnim = Random.Range(0, 3);
            switch (randomAnim)
            {
                case 0:
                    TrainCounter = 10;
                    return AnimKey.FightHit;

                case 1:
                    TrainCounter = 10;
                    return AnimKey.FightIdle;

                case 2:
                    TrainCounter = 6;
                    return AnimKey.FightKick;
                // case 3:
                //     TrainCounter = 6;
                //     return AntAnim.FightShovel1;
                // case 4:
                //     TrainCounter = 6;
                //     return AntAnim.FightShovel2;

                default: return AnimKey.None;
            }
        }
    }
}