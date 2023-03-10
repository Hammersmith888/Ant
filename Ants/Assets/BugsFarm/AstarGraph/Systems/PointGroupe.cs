using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsFarm.AstarGraph
{
    [RequireComponent(typeof(PointGenerator))]
    public class PointGroupe : MonoBehaviour
    {
        [Header("Настройка группы")]
        [Tooltip("Номер группы путей")]
        [SerializeField] private int _groupe = -1;
        [Tooltip("Ярлыки группы, позволяют фильтровать расположение точек в графах.")]
        [NodeTagsSelector] [SerializeField] private string _tag = string.Empty;

        [Header("Зависимость от групп")]
        [Tooltip("Является группой родителем?")]
        [SerializeField] private bool _isParent = false;
        
        public string Groupe => _groupe.ToString();
        public bool IsParent => _isParent;
        public PointGenerator PointGenerator => GetComponent<PointGenerator>();
        
        public IEnumerable<Node> GetPoints()
        {
            var data = new NodeData
            {
                Group = Groupe,
                Tag = (uint)NodeUtils.GetTagID(_tag),
            };
            return PointGenerator.GeneratePointsGroupe(data);
        }
        
        public IEnumerable<PointGroupe> GetGroups()
        {
            return GetComponentsInChildren<PointGroupe>();
        }
        
        public void Override(PointGroupe from)
        {
            _groupe = Int32.Parse(from.Groupe);
        }
        
        public void OnValidate()
        {
            if (_isParent)
            {
                var childs = GetComponentsInChildren<PointGroupe>();
                foreach (var child in childs)
                {
                    if (!child.Equals(this))
                        child.Override(this);
                }
            }      
        }

        [ExposeMethodInEditor]
        private void ClearUnUsed()
        {
            var childs = GetComponentsInChildren<Transform>(true);
            foreach (var child in childs)
            {
                if(!child) continue;
                
                if (!child.gameObject.activeSelf)
                {
                    Debug.LogError($"{this} : ClearUnUsed : {child.name}");
                    DestroyImmediate(child.gameObject);
                }
            }
        }        
        [ExposeMethodInEditor]
        private void FixZDepth()
        {
            var childs = GetComponentsInChildren<Transform>(true);
            foreach (var child in childs)
            {
                if(!child) continue;
                var pos = child.transform.position;
                if (pos.z != 0)
                {
                    Debug.LogError($"{this} : Fixed : {child.name}, ZDepth : {pos.z}");
                }
                pos.z = 0;
                child.transform.position = pos;
            }
        }
    }
}

