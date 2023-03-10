using BugsFarm.Graphic;
using BugsFarm.Services.StorageService;
using Spine.Unity;
using UnityEngine;

namespace BugsFarm.LeafHeapSystem
{
    public class LeafHeapSceneObject : MonoBehaviour, IStorageItem
    {
        public SkeletonAnimation MainSkeleton => _mainSkeleton;
        public VisibleTrigger VisibleTrigger => _visibleTrigger; 
        
        [SerializeField] private GameObject _selfContainer;
        [SerializeField] private SkeletonAnimation _mainSkeleton;
        [SerializeField] private VisibleTrigger _visibleTrigger;
        
        public string Id { get; set; }
        
        public void ChangeVisible(bool value)
        {
            if (_selfContainer)
            {
                _selfContainer.SetActive(value);
            }
        }
        
        private void OnValidate()
        {
            if (!_selfContainer)
            {
                _selfContainer = gameObject;
            }
        }
    }
}