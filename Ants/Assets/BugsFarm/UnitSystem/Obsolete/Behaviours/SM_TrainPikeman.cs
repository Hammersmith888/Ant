using System;
using BugsFarm.AnimationsSystem;
using BugsFarm.AudioSystem.Obsolete;
using BugsFarm.Game;
using Random = UnityEngine.Random;

namespace BugsFarm.UnitSystem.Obsolete
{
    [Serializable]
    public class SM_TrainPikeman : SM_ATrain, ISoundOccupant
    {
        protected override ObjType TrainObjectType => ObjType.str_Pikes;
        protected int RestCounter = 0;
        protected int TrainCounter = 0;
        protected AnimKey CurrentTrain = AnimKey.None;
        public bool CanSoundPlay => SoundCycle % 3 == 1;
        [NonSerialized] protected int SoundCycle;
        [field: NonSerialized] public event Action<Sound> OnSoundChange;
        [field: NonSerialized] public event Action<ISoundOccupant> OnSoundFree;

        public SM_TrainPikeman(int priority, StateAnt aiType) : base(priority, aiType) { }
        public override void PostLoadRestore()
        {
            base.PostLoadRestore();
            if (State != TrainState.Train) return;
            SetSound();
        }
        protected override void OnTrain()
        {
            if (TrainCounter <= 0)
            {
                Transition(TrainState.Rest);
                return;
            }
            TrainCounter--;
            base.OnTrain();
            SetSound();
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
            base.OnRest();
            FreeSound();
        }
        protected override void OnNone()
        {
            base.OnNone();
            FreeSound();
            TrainCounter = RestCounter = 0;
        }
        protected override AnimKey GetAnim()
        {
            switch (State)
            {
                case TrainState.GoTrain: return AnimKey.Walk;
                case TrainState.Train: return CurrentTrain;
                case TrainState.Rest: return AnimKey.Idle;
                default: return AnimKey.None;
            }
        }
        protected virtual AnimKey RandomTrain()
        {
            var randomAnim = Random.Range(0, 3);
            switch (randomAnim)
            {
                case 0: TrainCounter = 10; 
                    return AnimKey.Attack;

                case 1: TrainCounter = Random.Range(6, 13); 
                    return AnimKey.Attack2;

                case 2: TrainCounter = 8; 
                    return AnimKey.Training;

                default: return AnimKey.None;
            }
        }
        protected override void OnDestenationComplete()
        {
            base.OnDestenationComplete();
            if (TimerTask.IsReady)
                return;

            if (State == TrainState.GoTrain)
            {
                SetCycle();
                Transition(TrainState.Train);
            }
        }
        protected virtual void SetSound()
        {
            GameEvents.OnSoundQueue?.Invoke(this);
            OnSoundChange?.Invoke(Sound.PikemanTraining_1);
        }
        protected virtual void FreeSound()
        {
            OnSoundFree?.Invoke(this);
        }
        protected virtual void SetCycle()
        {
            SoundCycle++;
            CurrentTrain = RandomTrain();
            RestCounter = 5;
        }
    }
}
