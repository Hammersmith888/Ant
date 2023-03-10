using System;
using System.Collections.Generic;

namespace BugsFarm.BuildingSystem
{
    public class ReservedPlaceSystem : IReservedPlaceSystem
    {
        public event Action<string> OnPlaceFree; 
        public event Action<string> OnPlaceReserved; 
        private readonly Dictionary<string, string> _storage;
        
        public ReservedPlaceSystem()
        {
            _storage = new Dictionary<string, string>();
        }
        
        public IEnumerable<string> GetReservedPlaces()
        {
            return _storage.Keys;
        }
        
        public string GetPlaceOccupant(string placeNum)
        {
            return HasEntity(placeNum) ? _storage[placeNum] : null;
        }
        
        public void ReservePlace(string placeNum, string guid, bool notify)
        {
            if (HasEntity(placeNum))
            {
                throw new InvalidOperationException($"{this} Add :: PlaceNum {placeNum} , alredy reserved");
            }
            _storage.Add(placeNum,guid);
            if (notify)
            {
                OnPlaceReserved?.Invoke(placeNum);
            }
        }
        
        public void Remove(string placeNum, bool notify)
        {
            if (HasEntity(placeNum))
            {
                _storage.Remove(placeNum);
                if (notify)
                {
                    OnPlaceFree?.Invoke(placeNum);
                }
            }
        }
        public bool HasEntity(string placeNum)
        {
            return _storage.ContainsKey(placeNum);
        }
    }
}