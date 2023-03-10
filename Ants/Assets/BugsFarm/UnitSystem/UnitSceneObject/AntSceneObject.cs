using BugsFarm.BuildingSystem;
using BugsFarm.Graphic;
using Spine.Unity;
using UnityEngine;

namespace BugsFarm.UnitSystem
{
    public class AntSceneObject : TapableUnitSceneObject
    {
        public SkeletonAnimation ClimbSkeleton => _climbSkeleton;
        
        [SerializeField] private SkeletonAnimation _climbSkeleton;
        
        public override void SetAlpha(float alpha01)
        {
            base.SetAlpha(alpha01);
            
            if(!_climbSkeleton) return;
            _climbSkeleton.skeleton.A = alpha01;
        }

        public override void SetLayer(LocationLayer layer)
        {
            base.SetLayer(layer);
            
            if(!_climbSkeleton) return;
            _climbSkeleton.MeshRenderer.sortingLayerID = layer.ID;
            _climbSkeleton.MeshRenderer.sortingOrder = layer.Order;
        }
    }
}