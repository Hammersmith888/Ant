using System.Collections.Generic;
using BugsFarm.BuildingSystem;
using UnityEngine;

namespace BugsFarm.Prefabs.PlaceIDs
{
    public class CreatePlaceIDGroupe : MonoBehaviour
    {
        [ExposeMethodInEditor]
        private void CreateGroupes()
        {
            var palceIDs = GetComponentsInChildren<PlaceID>(true);
            var dict = new Dictionary<int, Transform>();
            foreach (var placeID in palceIDs)
            {
                if(!dict.ContainsKey(placeID.GroupID))
                {
                    var placeGroupe = new GameObject("PlaceIDGroup_" + placeID.GroupID + "_prefab").transform;
                    placeGroupe.SetParent(transform);
                    dict.Add(placeID.GroupID, placeGroupe);
                }
                var parent = dict[placeID.GroupID];
                placeID.transform.SetParent(parent);
            }
        }
    }
}
