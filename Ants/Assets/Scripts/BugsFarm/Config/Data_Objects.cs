using System;
using System.Collections.Generic;
using System.Linq;
using Malee.List;
using UnityEngine;

namespace BugsFarm.Config
{
    public class Data_Objects : MB_Singleton<Data_Objects>
    {
        public IEnumerable<CfgBuilding> Buildings => _buildings;
        public IEnumerable<CfgFood> Food => _foods;
        public IEnumerable<CfgDecor> Decors => _decorations;

        [Serializable]
        private class RDataBuildings : ReorderableArray<CfgBuilding>{}
        [Serializable]
        private class RDataFoods : ReorderableArray<CfgFood>{}
        [Serializable]
        private class RDataDecors : ReorderableArray<CfgDecor>{}
    
        [Reorderable][SerializeField] private RDataBuildings _buildings = new RDataBuildings();
        [Reorderable][SerializeField] private RDataFoods _foods = new RDataFoods();
        [Reorderable][SerializeField] private RDataDecors _decorations = new RDataDecors();

        public CfgObject GetData(ObjType type, int subType)
        {
            switch (type)
            {
                case ObjType.Food:       return GetData((FoodType) subType);
                case ObjType.Decoration: return GetData((DecorType) subType);
                default:                 return GetData(type);
            }
        }
        public CfgBuilding GetData(ObjType type)
        {
            return _buildings.FirstOrDefault(x => x.type == type);
        }
        public CfgFood GetData(FoodType type)
        {
            return _foods.FirstOrDefault(x => x.foodType == type);
        }
        public CfgDecor GetData(DecorType type)
        {
            return _decorations.FirstOrDefault(x => x.decorType == type);
        }
        public bool IsBig(ObjType type)
        {
            return GetData(type)?.isBig ?? false;
        }
    }
}