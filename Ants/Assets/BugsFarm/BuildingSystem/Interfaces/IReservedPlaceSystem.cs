using System;
using System.Collections.Generic;

namespace BugsFarm.BuildingSystem
{
    public interface IReservedPlaceSystem
    {
        event Action<string> OnPlaceFree;
        event Action<string> OnPlaceReserved;
        IEnumerable<string> GetReservedPlaces();
        string GetPlaceOccupant(string placeNum);
        void ReservePlace(string placeNum, string guid, bool notify);
        void Remove(string placeNum, bool notify);
        bool HasEntity(string placeNum);
    }
}