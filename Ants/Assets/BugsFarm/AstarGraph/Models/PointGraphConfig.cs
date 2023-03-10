using System.Collections.Generic;
using UnityEngine;

namespace BugsFarm.AstarGraph
{
    [CreateAssetMenu(fileName = "PointGraphConfig", menuName = "Config/Graph/PointGraphConfig")]
    public class PointGraphConfig : ScriptableObject
    {
        [SerializeField] private ScenePathModel[] _models;
        public IEnumerable<ScenePathModel> Items => _models;
    }
}