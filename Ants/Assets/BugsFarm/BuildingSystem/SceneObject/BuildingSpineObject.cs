using BugsFarm.Graphic;
using Spine.Unity;
using UnityEngine;

namespace BugsFarm.BuildingSystem
{
    public class BuildingSpineObject : BuildingSceneObject
    {
        public SkeletonAnimation MainSkeleton => _skeletonAnimation;
        [SerializeField] protected SkeletonAnimation _skeletonAnimation;
        public override void SetObject(params object[] args)
        {
            var skinIndex = (int)args[0];
            var skins = _skeletonAnimation.skeleton.Data.Skins.ToArray();
            
            if (skinIndex < skins.Length)
            {
                var skin = skins[skinIndex];
                _skeletonAnimation.skeleton.SetSkin(skin);
                _skeletonAnimation.skeleton.SetSlotsToSetupPose();
                _skeletonAnimation.AnimationState.Apply(_skeletonAnimation.skeleton);
            }
        }

        public override void SetLayer(LocationLayer layer)
        {
            base.SetLayer(layer);
            _skeletonAnimation.MeshRenderer.sortingLayerID = layer.ID;
            _skeletonAnimation.MeshRenderer.sortingOrder = layer.Order;
        }

        public override void SetAlpha(float alpha01)
        {
            _skeletonAnimation.skeleton.A = alpha01;
        }
    }
}