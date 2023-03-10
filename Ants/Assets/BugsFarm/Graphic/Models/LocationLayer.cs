using System;
using UnityEngine;

namespace BugsFarm.Graphic
{
    [Serializable]
    public struct LocationLayer
    {
        public int ID => _id;
        public int Order => _order;
        public string Name => _name;
        
        [SerializeField] private int _id;
        [SerializeField] private int _order;
        [SerializeField] private string _name;

        public LocationLayer(string name, int order)
        {
            _name = name;
            _order = order;
            _id = SortingLayers.NameToSortingID(name);
        }
        public LocationLayer(int id, int order)
        {
            _name = SortingLayers.SortingIDToName(id);
            _order = order;
            _id = id;
        }
    }
}