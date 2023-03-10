using System;

namespace BugsFarm.BuildingSystem
{
    [Serializable]
    public struct PlaceFiler
    {
        public bool Active;
        public APlace Place;

        public PlaceFiler(APlace place)
        {
            Place = place;
            Active = false;
        }
    }
}
