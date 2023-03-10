using System;
using System.Collections.Generic;

namespace BugsFarm.UnitSystem.Obsolete
{
    [Serializable]
    public abstract class AStateMachine<T> : ATask where T : Enum
    {
        protected bool TransitionToSameStateAllowed = true;

        public T State { get; private set; }
        public override bool IsTaskEnd => IsStateNone();
        public override void ForceHardFinish()
        {
            Transition(GetNone());
        }

        protected void Transition(T state, bool onEnterAction = true)
        {
            if (!TransitionToSameStateAllowed && EqualityComparer<T>.Default.Equals(State, state))
                throw new Exception($"Transition to the same state ({ State })");

            OnExit();

            State = state;

            if (onEnterAction)
                OnEnter();
        }
        protected virtual void OnExit() { }
        protected virtual void OnEnter(){}

        private T GetNone()
        {
            return (T)Enum.ToObject(typeof(T), 0);
        }
        private bool IsStateNone()
        {
            return EqualityComparer<T>.Default.Equals(State, GetNone());
        }
    }
}

