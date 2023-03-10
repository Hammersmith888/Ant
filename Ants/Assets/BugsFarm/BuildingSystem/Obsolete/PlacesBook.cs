using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BugsFarm.BuildingSystem.Obsolete
{
    /// <summary>
    /// Это дич какаето, нельзя пока трогать
    /// </summary>
    public class PlacesBook
    {
        private static readonly List<APlace> _places = new List<APlace>();
        public static void Add(APlace place)
        {
            //if (place.Type == ObjType.None || _places.Contains(place))
                //return;

            //_places.Add(place);
        }
        public static void Add(APlace[] places)
        {
            _places.AddRange(places);
        }
        public static void Remove(APlace place)
        {
            if (!_places.Contains(place))
            {
                Debug.LogError("Places does not exist!!!");
                return;
            }

            _places.Remove(place);
        }
        public static void Remove(APlace[] places)
        {
            foreach (var palce in places)
            {
                Remove(palce);
            }
        }
        public static APlace[] GetPlaces(ObjType type, int subType = 0)
        {
            return null; //_places.Where(x => x.IsOpen && x.Type == type && x.ConvertedSubtype() == subType).ToArray();
        }
        public static APlace GetPlace(int placeNum, ObjType type, int subType = 0)
        {
            return null; //_places.FirstOrDefault(x => x.IsOpen && x.Type == type && x.ConvertedSubtype() == subType && placeNum == x.PlaceNum);
        }
        public static T GetPlace<T>(int placeNum, ObjType type, int subType = 0) where T : APlace
        {
            return (T)GetPlace(placeNum, type, subType);
        }
        public static T GetPlace<T>(int placeNum) where T : APlace
        {
            return null; //_places.OfType<T>().FirstOrDefault(x => x.PlaceNum == placeNum);
        }
        public static T[] GetPlaces<T>() where T : APlace
        {
            return _places.OfType<T>().ToArray();
        }
    }
}

