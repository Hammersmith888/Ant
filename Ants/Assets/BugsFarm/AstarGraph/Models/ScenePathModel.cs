using BugsFarm.Services.StorageService;
using UnityEngine;

namespace BugsFarm.AstarGraph
{
    [CreateAssetMenu(fileName = "ScenePathModel", menuName = "Config/Graph/ScenePathModel")]
    public class ScenePathModel : ScriptableObject, IStorageItem
    {
        [SerializeField] private string _modelId;
        [SerializeField] private PointGroupe[] _pointGroups;
        [SerializeField] private PathLimitationArea[] _limitationAreas;
        public PointGroupe[] PointGroupe => _pointGroups;
        public PathLimitationArea[] LimitationAreas => _limitationAreas;
        
        string IStorageItem.Id => _modelId;
    }
}