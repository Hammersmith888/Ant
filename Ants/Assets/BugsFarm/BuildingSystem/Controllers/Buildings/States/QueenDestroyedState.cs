using BugsFarm.AnimationsSystem;
using BugsFarm.Services.StateMachine;
using UnityEngine;
using Zenject;

namespace BugsFarm.BuildingSystem.States
{
    public class QueenDestroyedState : State
    {
        public override string Id => "DeadBuildingState";

        private AnimKey _deathKey = AnimKey.Sleep;
        private ISpineAnimator _animator;

        [Inject]
        private void SetDependencies(ISpineAnimator animator)
        {
            _animator = animator;
        }
        
        public override void OnEnter(params object[] args)
        {
            _animator.SetAnim(_deathKey);
            Debug.Log("On dead state entered");
        }

        public override void OnExit()
        {
            Debug.Log("From dead state exit");
        }
    }
}