using System.Collections.Generic;
using System.Linq;
using BugsFarm.BuildingSystem.Obsolete;
using UnityEngine;

namespace BugsFarm.Objects.Stock.Utils
{
    public enum StockCheck { Anyway, Full, Depleted }
    public static class Stock
    {
        private static MonoFactory _monoFactory;
        public static void Init(MonoFactory monoFactory)
        {
            _monoFactory = monoFactory;
        }
        public static bool TrySpawn(ObjType type, int subType, out IStock stock)
        {
            var success = TryFindPlace(type, subType, out var placeNum);
            stock = success ? (IStock)_monoFactory.Spawn(placeNum, type, subType) : null;
            return success;
        }
        public static bool TryFindPlace(ObjType type, int subType, out int randomPlaceNum, List<int> excluding = null)
        {
            var available = new List<int>();

            foreach (var place in PlacesBook.GetPlaces(type, subType))
            {
                //var placeNum = place.PlaceNum;

                //if ((excluding.IsNullOrDefault() || !excluding.Contains(placeNum)) &&
                    //OccupiedPlaces.IsFree(placeNum, type))
               // {
                    //available.Add(placeNum);
                //}
            }

            var success = available.Any();
            randomPlaceNum = success ? available[Random.Range(0, available.Count)] : 0 ;
            return success;
        }
        public static T FindMore<T>(this T[] stocks, StockCheck check) where T : IStock
        {
            if (stocks.IsNullOrDefault() || stocks.Length == 0) return default;
            
            var bestMatch = stocks[Random.Range(0, stocks.Length)];
            foreach (var stock in stocks)
            {
                switch (check)
                {
                    case StockCheck.Anyway: return bestMatch;
                    case StockCheck.Full:
                        if (stock.QuantityCur > bestMatch.QuantityCur)
                        {
                            bestMatch = stock;
                        }
                        break;
                    case StockCheck.Depleted:
                        if (stock.QuantityCur < bestMatch.QuantityCur)
                        {
                            bestMatch = stock;
                        }
                        break;
                }
            }
            return bestMatch;
        }
        public static IStock[] Find(ObjType type, int subType)
        {
            return Keeper.GetObjects(type).Where(x => x.SubType == subType).OfType<IStock>().ToArray();
        }
        public static T[] Find<T>()
        {
            return Keeper.GetObjects<T>().ToArray();
        }
    }
}