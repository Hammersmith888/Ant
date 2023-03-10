using System.Collections.Generic;
using UnityEngine;

namespace BugsFarm.Graphic
{
    public static class SortingLayers 
    {
        private static readonly List<string> _layers = new List<string>();
        private static void Init()
        {
            if(_layers.Count != 0) return;
            
            foreach (var sortingLayer in SortingLayer.layers)
            {
                _layers.Add(sortingLayer.name);
            }
        }
        public static int NameToLayerIndex(string layerName)
        {
            Init();
            return !_layers.Contains(layerName) ? 0 : _layers.IndexOf(layerName);
        }

        public static string LayerIndexToName(int layerIndex)
        {
            Init();
            return _layers.Count > layerIndex ? _layers[layerIndex] : "Default";
        }
        
        public static int LayerIndexToSortingID(int layerIndex)
        {
            Init();
            return _layers.Count > layerIndex ? NameToSortingID(_layers[layerIndex]) : 0;
        }

        public static int NameToSortingID(string layerName)
        {
            Init();
            return SortingLayer.NameToID(layerName);
        }
        public static string SortingIDToName(int layerID)
        {
            Init();
            return SortingLayer.IDToName(layerID);
        }
    }
}