using System;
using BugsFarm.AnimationsSystem;
using BugsFarm.SimulationSystem.Obsolete;
using UnityEngine;

/*
	Important!
		https://stackoverflow.com/questions/9115477/auto-implemented-properties-and-serialization
		https://stackoverflow.com/a/9115576/4830242
*/


namespace BugsFarm.UnitSystem.Obsolete
{
    [Serializable]
    public class Walker : IPostLoadRestorable
    {
    #region Getters
        public bool InQueue => WaitForLadders || IsWalking && _antAnimator.CurrentAnim == AnimKey.Idle;
        public bool WaitForLadders => _occupier.WaitForLadders;

        public GPos gPos => _gPos;
        public GPos gPos_Tgt => _gPos_Tgt;

        public bool IsLadder => _gPos.gVert && _gPos.gVert.IsLadder;

        public GStep CurStep => _path.CurStep;
        public GStep NextStep => _path.NextStep;

        public Transform Agent => _agent;
        public GPath Path => _path;

    #endregion
    #region NonSerialized

        [NonSerialized] private GPos _gPos;
        [NonSerialized] private GPos _gPos_Tgt;

        [NonSerialized] private Transform _agent;
        [NonSerialized] private AntAnimator _antAnimator;

        [NonSerialized] private GPath _path;
        [NonSerialized] private Occupier _occupier;

    #endregion
    #region Serialized
        public int ID { get; private set; }
        public bool IsWalking { get; private set; }
        public bool CanJump { get; private set; }
        public bool DontCheckLadders { get; private set; }

        private AntType _type;
        private SGPos _pos_Cur;
        private SGPos _pos_Tgt;
        private Jumper _jumper = new Jumper();
    #endregion

        public Walker(AntType type)
        {
            ID = Keeper.AntIDs.GetFreeKey();

            _type = type;
            CanJump = false;
            DontCheckLadders = type == AntType.Worker;
        }
        public void Dispose()
        {
            Keeper.AntIDs.ReturnKey(ID);

            _occupier.Dispose();
        }


        //[OnSerializing]
        //internal void OnSerializingMethod(StreamingContext context)
        //{
        //    _pos_Cur = _gPos;
        //    _pos_Tgt = _gPos_Tgt;
        //}

        //[OnDeserialized]
        //internal void OnDeserializedMethod(StreamingContext context)
        //{
        //    _gPos = _pos_Cur;
        //    _gPos_Tgt = _pos_Tgt;
        //}

        public void Init(Transform agent, AntAnimator antAnimator)
        {
            _agent = agent;
            _antAnimator = antAnimator;

            _path = new GPath();
            _occupier = new Occupier(this, _path);

            _jumper.Init(agent, antAnimator);
        }
        public void PostLoadRestore()
        {
            if (IsWalking)
                Go(gPos_Tgt);
        }
        public void SetPos(GPos gPos)
        {
            _gPos = gPos;

            _agent.position = _agent.position.SetXY(gPos.GetPoint());
        }

        public void TeleportToRandom()
        {
            GPos target = GetRandomTarget();

            _gPos = target;
            _agent.position = target.GetPoint();

            _occupier.cb_Go(target);
        }
        public void GoToRandom()
        {
            GPos target = GetRandomTarget();

            Go(target);
        }
        public void Go(GPos target)
        {
            if (!_path.Find(_gPos, target))
                return;

            IsWalking = true;
            _gPos_Tgt = target;

            _occupier.cb_Go(target);

            //_antAnimator.SetAnim(_antAnimator.WalkAnim);

            SetLookDir();
        }
        public void Stay()
        {
            IsWalking = false;

            // if (Simulation.type != SimulationType.Raw)		// (!) not checking, we don't want blocked Walking Positions after Demo-simulation
            _occupier.cb_Stay();

            _antAnimator.SetAnim(AnimKey.Idle);
        }
        public void Update()
        {
            if (!IsWalking)
                return;


            if (SimulationOld.Type == SimulationType.Raw)
            {
                Debug.LogError("ReachTarget : SimulationType.Raw");
                ReachTarget();
                _agent.position = _gPos.GetPoint();
                return;
            }


            if (_jumper.IsJumping)
            {
                _jumper.Update();

                if (!_jumper.IsJumping)
                    _gPos = new GPos(CurStep.gVert, CurStep.t1);
            }
            else
            {
                if (CheckLadders(true))
                    return;


                float speed = GetSpeed(out float speedMul);
                float dist = speed * SimulationOld.DeltaTime;


                int cycle = 0;
                while (IsWalking && cycle++ < 10 && dist > 0) // just to be on a safe side
                    ConsumeDist(ref dist);


                if (_type != AntType.Snail && _type != AntType.Spider)
                    _occupier.UpdateCurWalkPos(_agent.position);


                if (IsWalking)
                    _antAnimator.Apply_SpeedMul(speedMul, WaitForLadders);
            }
        }

        private GPos GetRandomTarget()
        {
            bool grassOnly = _type == AntType.Spider || _type == AntType.Snail;
            GVert except = grassOnly ? null : gPos.gVert;
            GPos target = Graph.Instance.RandomRoad(except, true, grassOnly);

            return target;
        }
        private bool IsGoingOnLadder(bool checkDist)
        {
            return
                !_path.IsLastStep() &&
                !CurStep.IsLadder &&
                NextStep.IsLadder &&
                (
                !CanJump ||
                !NextStep.IsLadderDown
                ) &&
                (
                !checkDist ||
                Vector2.Distance(_agent.position, CurStep.PointAt_t1) < Constants.LadderCheckDist
                )
                ;
        }
        private bool CheckLadders(bool checkDist)
        {
            if (_occupier.LaddersOccupied || !IsGoingOnLadder(checkDist))
                return WaitForLadders;


            bool WFL_changed = _occupier.CheckLadders(DontCheckLadders);

            //if (WFL_changed)
            //_antAnimator.SetAnim(WaitForLadders ? AntAnim.Breath : _antAnimator.WalkAnim);

            return WaitForLadders;
        }
        private float GetSpeed(out float speedMul)
        {
            speedMul = _type == AntType.Snail ||  _type == AntType.Spider ? 1 : WalkerHelper.CalcSpeedMul(this);

            switch (_type)
            {
                case AntType.Snail:  return Constants.Snail_Speed;
                case AntType.Spider: return Constants.Spider_Speed;
                default:             return Constants.Ant_Speed * speedMul;
            }
        }
        private void ConsumeDist(ref float dist)
        {
            float dir = Mathf.Sign(CurStep.Delta_t);

            _gPos.t += dist / CurStep.gVert.Length * dir;

            bool overshoot = Mathf.Abs(_gPos.t - CurStep.t0) > Mathf.Abs(CurStep.Delta_t) ;

            if (overshoot)
            {
                if ( !_gPos.gVert.IsLadder && CheckLadders(false) )
                {
                    // Wait for ladders

                    _gPos.t = CurStep.t1;
                    overshoot = false;
                }
                else
                {
                    if (_path.Proceed())
                    {
                        // Done

                        ReachTarget();
                    }
                    else
                    {
                        // Next step

                        // Free ladder
                        if (_gPos.gVert.IsLadder)
                            _occupier.LadderFree();

                        _gPos = new GPos(CurStep.gVert, CurStep.t0);

                        if (CanJump && CurStep.IsLadderDown)
                        {
                            overshoot = false;

                            _jumper.Jump(CurStep);
                        }
                        else
                            // _antAnimator.SetActive(IsLadder);

                            SetLookDir();
                    }
                }
            }

            ConsumeDist(_gPos.GetPoint(), overshoot, ref dist);
        }
        private void ReachTarget()
        {
            _gPos = _gPos_Tgt;

            Stay();
        }
        private void ConsumeDist(Vector2 pos, bool overshoot, ref float dist)
        {
            Vector2 delta = pos - (Vector2)_agent.position;
            _agent.position = pos;

            dist = overshoot ? dist - delta.magnitude : 0 ;
        }
        private void SetLookDir()
        {
            if (CurStep.gVert.IsLadder)
                return;

            float delta = CurStep.PointAt_t1.x - CurStep.PointAt_t0.x;
        }
    }
}

