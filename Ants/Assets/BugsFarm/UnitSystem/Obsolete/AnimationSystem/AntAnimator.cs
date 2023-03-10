using System;
using BugsFarm.AnimationsSystem;
using BugsFarm.Game;
using BugsFarm.SimulationSystem.Obsolete;
using Spine;
using Spine.Unity;

namespace BugsFarm.UnitSystem.Obsolete
{
    public class AntAnimator
    {
        public bool IsAnimComplete => _playerMain.IsAnimComplete;
        public AnimKey CurrentAnim { get; private set; }
        public SkeletonAnimation SkeletonCurrent { get; private set; }
        public event Action<Event> OnSpineAnimationEvent;
        private SortingLayer CurrentLayer { get; set; }

        private readonly UnitRipSceneObject _unitRipSceneObject;
        private readonly SkeletonAnimation _spineMain;
        private readonly SkeletonAnimation _spineClimb;
        private readonly AnimRefs_Ant _animRefs;
        private readonly AnimPlayer _playerMain;
        private readonly AnimPlayer _playerClimb;
        private struct SortingLayer
        {
            public int Id;
            public int Order;
        }
        private bool _playBackground;
        public AntAnimator(AntType type, SkeletonAnimation spineMain, SkeletonAnimation spineClimb, UnitRipSceneObject unitRipSceneObject)
        {
            _unitRipSceneObject = unitRipSceneObject;
            _spineMain = spineMain;
            _spineClimb = spineClimb;

            _playerMain = new AnimPlayer(spineMain);
            _playerClimb = new AnimPlayer(spineClimb);

            _animRefs = Data_Fight.Instance.units[type].animations;

            if (_spineClimb)
            {
                _spineClimb.AnimationState.Event += AnimationState_Event;
            }
            else if (_spineMain)
            {
                _spineMain.AnimationState.Event += AnimationState_Event;
            }
            GameEvents.OnSimulationEnd += OnSimulationEnd;
        }
        public void SetSortingLayer(int layerID, int layerOrder = 0)
        {
            if (SkeletonCurrent && !SimulationOld.Raw)
            {
                SkeletonCurrent.MeshRenderer.sortingLayerID = layerID;
                SkeletonCurrent.MeshRenderer.sortingOrder = layerOrder;
            }
            CurrentLayer = new SortingLayer{Id = layerID, Order = layerOrder};
        }
        public void SetAnim(AnimKey animation, float timeScaleMul = 1)
        {
            if (animation.IsNullOrDefault())
            {
                return;
            }

            CurrentAnim = animation;
            if (!SimulationOld.Raw)
            {
                SetAnim(timeScaleMul);
            }
        }
        public void Apply_SpeedMul(float speedMul, bool waitingForLadders)
        {
            var breathing = CurrentAnim == AnimKey.Idle;


            // Set timeScale for walk/climb animations
            if (!breathing)
                _playerMain.TimeScale = GetTimeScale(CurrentAnim) * speedMul;

            if (_playerClimb.Initialized)
                _playerClimb.TimeScale = GetTimeScale(AnimKey.WalkClimb) * speedMul;


            // Change animation: "Breath" - if too slow, back to "Walk" - if started to move faster
            //if (!waitingForLadders)
            //{
            //    if ( !breathing &&  speedMul < Constants.MinSpeedMul )
            //        SetAnim(AntAnim.Breath);
            //    else if ( breathing && speedMul >= Constants.MinSpeedMul )
            //        SetAnim(WalkAnim);
            //}
        }
        public void FastForward()
        {
            _playerMain.FastForward();
            _playerClimb?.FastForward();
        }
        public bool HasAnim(AnimKey anim)
        {
            return !_animRefs.GetAnim(anim).IsNullOrDefault();
        }

        public void Update(bool isLadder, bool isFlyable)
        {
            _spineMain.gameObject.SetActive(!isLadder);
            _spineClimb?.gameObject.SetActive(isLadder);
            SkeletonCurrent = isLadder ? _spineClimb ?? _spineMain : _spineMain;
            
            if(isFlyable && !_playBackground)
            {
                var interupted = CurrentAnim;
                SetAnim(AnimKey.Fly);
                CurrentAnim = interupted;
                _playBackground = true; 

            }
            else if (!isFlyable && _playBackground)
            {
                _playBackground = false;
                SetAnim(CurrentAnim);
            }
        }

        private bool IsLoop()
        {
            switch (CurrentAnim)
            {
                case AnimKey.Attack:
                case AnimKey.Attack2:
                case AnimKey.FightShovel1:
                case AnimKey.FightShovel2:
                case AnimKey.FightHit:
                case AnimKey.FightKick:

                case AnimKey.Death:
                case AnimKey.Awake:
                case AnimKey.MineLever:
                case AnimKey.DropMud:
                case AnimKey.GiveFood:
                case AnimKey.TakeFoodLow:
                case AnimKey.TakeFoodMid:
                case AnimKey.TakeFoodHi:
                case AnimKey.TakeGarbage:
                case AnimKey.GarbageDropPile:
                case AnimKey.GarbageDropRecycler:
                case AnimKey.JumpOff:
                case AnimKey.JumpLand:
                    return false;

                default:
                    return true;
            }
        }
        private float GetTimeScale(AnimKey animation)
        {
            return _animRefs.GetTimeScale(animation);
        }
        private void SetAnim(float timeScaleMul = 1)
        {
            if(_playBackground) return;
            var timeScale = GetTimeScale(CurrentAnim) * timeScaleMul;
            var ara = _animRefs.GetAnim(CurrentAnim);

            var player = CurrentAnim == AnimKey.WalkClimb ? _playerClimb : _playerMain;

            player.Play(ara.name, IsLoop(), timeScale);
        }
        private void AnimationState_Event(TrackEntry trackEntry, Event e)
        {
            OnSpineAnimationEvent?.Invoke(e);
        }
        private void OnSimulationEnd()
        {
            if(CurrentAnim.IsNullOrDefault()) return;
            SetSortingLayer(CurrentLayer.Id, CurrentLayer.Order);
            SetAnim();

        }
    }
}