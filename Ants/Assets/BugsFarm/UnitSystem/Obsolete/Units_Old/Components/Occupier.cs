using System;
using BugsFarm.UnitSystem.Obsolete.Components;
using UnityEngine;

namespace BugsFarm.UnitSystem.Obsolete
{
    public class Occupier : IDisposable
    {
        public bool LaddersOccupied { get; private set; }
        public bool WaitForLadders { get; private set; }

        private Walker _walker;
        private GPath _path;

        public Occupier(Walker walker, GPath path)
        {
            _walker = walker;
            _path = path;
        }
        public void Dispose()
        {
            Occupied.WalkPos_Free(_walker);
            Occupied.Target_Free(_walker);
            Ladder_FreeAll();
        }
        public void cb_Go(GPos target)
        {
            WaitForLadders = false;
            Ladder_FreeAll();

            Occupied.Target_Occupy(_walker, target);
        }
        public void cb_Stay()
        {
            WaitForLadders = false;

            Occupied.WalkPos_Free(_walker);
            // Occupied.Target_Free		( _walker );		// No need
            Ladder_FreeAll();

            Occupied.Target_Occupy(_walker, _walker.gPos);
        }
        public void UpdateCurWalkPos(Vector2 pos)
        {
            if (_walker.IsWalking)
                Occupied.WalkPos_Occupy(_walker, _path.CurStep, pos, _walker.gPos.t);
        }
        public bool CheckLadders(bool dontCheckLadders)
        {
            bool before = WaitForLadders;

            // Check ladders
            if (!dontCheckLadders)
                WaitForLadders = IsLaddersOccupied();

            // Occupy ladders
            if (!WaitForLadders)
                LaddersOccupy();

            bool isChanged = WaitForLadders != before;

            return isChanged;
        }
        public void LadderFree()
        {
            LaddersOccupied = false;

            Occupied.Ladder_Free(_walker, _walker.gPos.gVert);
        }

        private bool IsLaddersOccupied()
        {
            int i = _path.Step + 1;

            while (_path[i].IsLadder)
                if (Occupied.Ladder_IsOccupied(_path[i++]))
                    return true;

            return false;
        }
        private void LaddersOccupy()
        {
            LaddersOccupied = true;

            int i = _path.Step + 1;

            while (_path[i].IsLadder)
                Occupied.Ladder_Occupy(_walker, _path[i++]);
        }
        private void Ladder_FreeAll()
        {
            LaddersOccupied = false;
            Occupied.Ladder_FreeAll(_walker);
        }
    }
}

