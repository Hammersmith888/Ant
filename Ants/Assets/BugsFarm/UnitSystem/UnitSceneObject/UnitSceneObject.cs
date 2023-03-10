using System.Linq;
using BugsFarm.Graphic;
using BugsFarm.Services.StorageService;
using Spine.Unity;
using UnityEngine;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public class UnitSceneObject : MonoBehaviour, IStorageItem
    {
        public SkeletonAnimation MainSkeleton => _mainSkeleton;
        public VisibleTrigger VisibleTrigger => _visibleTrigger;
        public string Id { get; private set; }
        [SerializeField] protected SkeletonAnimation _mainSkeleton;
        [SerializeField] protected VisibleTrigger _visibleTrigger;
        
        [Inject]
        private void Inject(string guid)
        {
            Id = guid;
        }
        
        public virtual void SetAlpha(float alpha01)
        {
            if(!_mainSkeleton) 
                return;
            
            _mainSkeleton.skeleton.A = alpha01;
        }
        
        public virtual void SetActive(bool active)
        {
            if (gameObject == null)
                return;
            gameObject.SetActive(active);
        }
        
        public virtual void SetLayer(LocationLayer layer )
        {
            if(!_mainSkeleton) 
                return;
            
            _mainSkeleton.MeshRenderer.sortingLayerID = layer.ID;
            _mainSkeleton.MeshRenderer.sortingOrder = layer.Order;
        }
        
        public virtual void SetInteraction(bool active){}

        public virtual void SetObject(params object[] args)
        {
            if(args.IsNullOrDefault() || args.Length == 0) return;

            var dataAsset = args.OfType<SkeletonDataAsset>().FirstOrDefault();
            if (!dataAsset) return;
            _mainSkeleton.skeletonDataAsset = dataAsset;
            _mainSkeleton.Initialize(true);
        }
    }
}