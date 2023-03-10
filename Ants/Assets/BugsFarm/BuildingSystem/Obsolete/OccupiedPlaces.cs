using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.Config;

namespace BugsFarm.BuildingSystem.Obsolete
{
    public static class OccupiedPlaces
    {
        public static readonly Dictionary<int, int[]> Neighbors = new Dictionary<int, int[]>
        {
            { 10,   new[]{  21                  }},
            { 11,   new[]{  22                  }},
            { 13,   new[]{  32,		/* 25 */	}},
            { 15,   new[]{  28                  }},
        };
        private static readonly Dictionary<int, APlaceable> _occupied = new Dictionary<int, APlaceable>();

        public static void Clear()
        {
            _occupied.Clear();
        }
        public static void GetOccupants(int placeNum, ObjType type, HashSet<APlaceable> occupants)
        {
            occupants.Clear();

            if (_occupied.ContainsKey(placeNum))
                occupants.Add(_occupied[placeNum]);

            if (!NeighborsAffected(placeNum, type))
                return;

            foreach (int neighbor in Neighbors[placeNum])
                if (_occupied.ContainsKey(neighbor))
                    occupants.Add(_occupied[neighbor]);
        }
        public static bool IsFree(int placeNum, ObjType type)
        {
            if (_occupied.ContainsKey(placeNum))
                return false;

            if (!NeighborsAffected(placeNum, type))
                return true;

            return !Neighbors[placeNum].Any(_occupied.ContainsKey);
        }
        public static void Occupy(APlaceable placeable)
        {
            Apply(placeable, Occupy_1place);
        }
        public static void Free(APlaceable placeable)
        {
            Apply(placeable, Free_1place);
        }
        public static bool NeighborsAffected(int placeNum, ObjType type)
        {
            return Data_Objects.Instance.IsBig(type) && Neighbors.ContainsKey(placeNum);
        }

        private static void Apply(APlaceable placeable, Action<int, APlaceable> action)
        {
            int placeNum = placeable.PlaceNum;

            action(placeNum, placeable);

            if (NeighborsAffected(placeNum, placeable.Type))
                foreach (int neighbor in Neighbors[placeNum])
                    action(neighbor, placeable);
        }
        private static void Occupy_1place(int placeNum, APlaceable placeable)
        {
            _occupied.Add(placeNum, placeable);
        }
        private static void Free_1place(int placeNum, APlaceable placeable)
        {
            _occupied.Remove(placeNum);
        }
    }
}

