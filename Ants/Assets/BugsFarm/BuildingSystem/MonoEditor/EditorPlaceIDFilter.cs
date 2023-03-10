using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.Graphic;
using Malee.List;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace BugsFarm.BuildingSystem
{
    [ExecuteInEditMode]
    public class EditorPlaceIDFilter : MonoBehaviour
    {
        [Header("Фильтры для поиска")]
        [SerializeField] private bool _restoreToActive = false;
        [SerializeField] private List<PlaceFiler> _filters = new List<PlaceFiler>();
        private readonly List<APlace> _filtred = new List<APlace>();

        [ExposeMethodInEditor]
        private void LoadPrefabFilters()
        {
            _filters.Clear();
        
            foreach (var item in Resources.LoadAll<APlace>("Places/"))
            {
                if(item is APlace place)
                {
                    _filters.Add(new PlaceFiler(place));
                }
            }
        }

        [ExposeMethodInEditor]
        private void ApplyFilters()
        {
            if (_filters != null && _filters.Count > 0)
            {
                _filtred.Clear();
                foreach (var item in GetComponentsInChildren<APlace>(true))
                {
                    var filter = _filters.First(x => item.ModelID == x.Place.ModelID);
                    item.gameObject.SetActive(filter.Active);
                    if(filter.Active)
                    {
                        _filtred.Add(item);
                    }
                }
            }
        }
        [ExposeMethodInEditor]
        private void RestoreFilters()
        {
            var places = GetComponentsInChildren<APlace>(true);
            foreach (var item in places)
            {
                item.gameObject.SetActive(_restoreToActive);
            }
            _filtred.Clear();
        }

        [ExposeMethodInEditor]
        private void SelectFiltredPlaceID()
        {
            if (_filtred != null && _filtred.Count > 0)
                Selection.objects = _filtred.Select(x => x.transform.parent.gameObject).ToArray();
        }

        [ExposeMethodInEditor]
        private void SelectFiltredAPlace()
        {
            if (_filtred != null && _filtred.Count > 0)
                Selection.objects = _filtred.Select(x => x.gameObject).ToArray();
        }
        [ExposeMethodInEditor]
        private void SortPlaceIDs()
        {
            var placeIds = GetComponentsInChildren<PlaceID>();
            placeIds = placeIds.OrderBy(x => int.Parse(x.PlaceNumber)).ToArray();
            var index = 0;
            foreach (var placeId in placeIds)
            {
                placeId.transform.SetSiblingIndex(index++);
            }
        }
        [ExposeMethodInEditor]
        private void SelectNonGrouped()
        {
            var nonGrouped = GetComponentsInChildren<PlaceID>().Where(x => x.GroupID < 0).Select(x=> x.gameObject).ToArray();
            if (nonGrouped.Length > 0)
                Selection.objects = nonGrouped;
        }

        [ExposeMethodInEditor]
        private void CleanVisiblePlaceIds()
        {
            var placeIDs = GetComponentsInChildren<PlaceID>();
            foreach (var placeID in placeIDs)
            {
                var childCount = placeID.transform.childCount;
                for (var i = 0; i < childCount; i++)
                {
                    var child = placeID.transform.GetChild(0);
                    DestroyImmediate(child.gameObject);
                }
            }
        }
        [ExposeMethodInEditor]
        private void ResetVisibleTransformations()
        {
            var placeIDs = GetComponentsInChildren<PlaceID>();
            foreach (var placeID in placeIDs)
            {
                var childCount = placeID.transform.childCount;
                for (var i = 0; i < childCount; i++)
                {
                    var child = placeID.transform.GetChild(0);
                    child.rotation = Quaternion.identity;
                    child.localPosition = Vector3.zero;
                    child.localScale = Vector3.one;
                }
            }
        }

        [SerializeField] [SortingLayerSelector] private string _sortingLayer;
        [SerializeField] private int _sortingLayerOrder = -1;

        [ExposeMethodInEditor]
        private void SetLayerVisible()
        {
            var layerID = SortingLayers.NameToSortingID(_sortingLayer);
            var placeIDs = GetComponentsInChildren<PlaceID>();
            foreach (var placeID in placeIDs)
            {
                var renderers = placeID.GetComponentsInChildren<Renderer>();
                foreach (var targetRenderer in renderers)
                {
                    targetRenderer.sortingLayerID = layerID;
                }
            }
        }
        [ExposeMethodInEditor]
        private void SetLayerOrderVisible()
        {
            var placeIDs = GetComponentsInChildren<PlaceID>();
            foreach (var placeID in placeIDs)
            {
                var renderers = placeID.GetComponentsInChildren<Renderer>();
                foreach (var targetRenderer in renderers)
                {
                    targetRenderer.sortingOrder = _sortingLayerOrder;
                }
            }
        }

        [ExposeMethodInEditor]
        private void RestorePrefabNameVisible()
        {
            var places = GetComponentsInChildren<APlace>();
            foreach (var place in places)
            {
                var sourcePlace = _filters.FirstOrDefault(x => x.Place.ModelID == place.ModelID);
                if (!sourcePlace.IsNullOrDefault())
                {
                    place.name = sourcePlace.Place.name;
                }
            }
        }
        private Vector3 GetCenter(GameObject[] playersInGame)
        {
            var totalX = 0f;
            var totalY = 0f;
            foreach (var player in playersInGame)
            {
                totalX += player.transform.position.x;
                totalY += player.transform.position.y;
            }
            var centerX = totalX / playersInGame.Length;
            var centerY = totalY / playersInGame.Length;
            return new Vector2(centerX, centerY);
        }

        [Serializable]
        public class ReorderableFilterPalces : ReorderableArray<PlaceFiler> { }
    }
}
#endif