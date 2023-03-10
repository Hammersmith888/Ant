using System;
using BugsFarm.AnimationsSystem;
using BugsFarm.AudioSystem.Obsolete;
using BugsFarm.Game;
using BugsFarm.SimulationSystem.Obsolete;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BugsFarm.UnitSystem.Obsolete
{
    [Serializable]
    public class SM_TrainArcher : SM_ATrain, ISoundOccupant
    {
        public virtual bool CanSoundPlay => _soundCycle % 3 == 1;
        [field: NonSerialized] public virtual event Action<Sound> OnSoundChange;
        [field: NonSerialized] public virtual event Action<ISoundOccupant> OnSoundFree;
        
        [NonSerialized] private int _soundCycle;
        [NonSerialized] private Transform _arrowPoint;
        protected override ObjType TrainObjectType => ObjType.str_ArrowTarget;
        private int _restCounter = 0;
        private int _trainCounter = 0;

        
        public SM_TrainArcher(int priority, StateAnt aiType) : base(priority, aiType) { }
        public override void Setup(SM_AntRoot root)
        {
            base.Setup(root);
            _arrowPoint = root.ArrowPoint;
        }
        public override void PostLoadRestore()
        {
            base.PostLoadRestore();
        
            if (State != TrainState.Train) return;
            SetSound();
        }
        public override void Dispose()
        {
            Animator.OnSpineAnimationEvent -= OnAnimationEventHandled;
            base.Dispose();
        }
        public override bool TaskStart()
        {
            if (base.TaskStart())
            {
                Animator.OnSpineAnimationEvent += OnAnimationEventHandled;
                return true;
            }

            return false;
        }

        protected override void OnGoTrain()
        {
            base.OnGoTrain();
            OnSoundFree?.Invoke(this);
        }
        protected override void OnTrain()
        {
            if (_trainCounter <= 0)
            {
                Transition(TrainState.Rest);
                return;
            }
            _trainCounter--;
            base.OnTrain();
            SetSound();
        }
        protected override void OnRest()
        {
            if (_restCounter <= 0)
            {
                SetCycle();
                Transition(TrainState.Train);
                return;
            }
            _restCounter--;
            base.OnRest();
            OnSoundFree?.Invoke(this);
        }
        protected override void OnNone()
        {
            base.OnNone();
            _trainCounter = _restCounter = 0;
            Animator.OnSpineAnimationEvent -= OnAnimationEventHandled;
        }

        protected override AnimKey GetAnim()
        {
            switch (State)
            {
                case TrainState.GoTrain: return AnimKey.Walk;
                case TrainState.Train: return AnimKey.Attack;
                case TrainState.Rest:  return AnimKey.Idle;
                default: return AnimKey.None;
            }
        }
        protected void OnAnimationEventHandled(Spine.Event e) 
        {
            if (SimulationOld.Any)
                return;

            if (State != TrainState.Train) return;
        
            if(e.Data.Name == "Shoot")
            {
                ShootArrow();
            }
        }
        protected override void OnDestenationComplete()
        {
            base.OnDestenationComplete();
            if (TimerTask.IsReady) 
                return;
            
            if(State == TrainState.GoTrain)
            {
                SetCycle();
                Transition(TrainState.Train);
            }

        }
        protected void SetSound(Sound? sound = null)
        {
            if (State != TrainState.Train) return;

            GameEvents.OnSoundQueue?.Invoke(this);
            sound = sound ?? Tools.RandomItem(Sound.Archer_BowString1, Sound.Archer_BowString2);
            OnSoundChange?.Invoke(sound.Value);
        }
        private void ShootArrow()
        {
            Arrows.Spawn(_arrowPoint.position, Equipment.Mb.transform.position);
            SetSound(Tools.RandomItem(Sound.Archer_Shoot1, Sound.Archer_Shoot2));
        }
        private void SetCycle()
        {
            _soundCycle++;
            _trainCounter = 3;
            _restCounter = Random.Range(4, 6 + 1);
        }
    }
}
