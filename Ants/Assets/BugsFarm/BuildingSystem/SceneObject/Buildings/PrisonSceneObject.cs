using BugsFarm.Graphic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Rendering;

namespace BugsFarm.BuildingSystem
{
    public class PrisonSceneObject : BuildingSpriteObject
    {
        public Transform PopupPoint => _popupPoint;
        public SkeletonAnimation SkeletonAnimation => _placeHolderAnimation;
        [SerializeField] private SpriteRenderer _rods;
        [SerializeField] private SortingGroup _sortingGroup;
        [SerializeField] protected SkeletonAnimation _placeHolderAnimation;
        [SerializeField] private Transform _popupPoint;
        
        public override void SetAlpha(float alpha01)
        {
            base.SetAlpha(alpha01);
            
            var sourceColor = _rods.color;
            sourceColor.a = alpha01;
            _rods.color = sourceColor;
        }

        public override void SetLayer(LocationLayer layer)
        {
            Layer = layer;
            _sortingGroup.sortingOrder = layer.Order;
            _sortingGroup.sortingLayerID = layer.ID;
        }

        public void SetSkeletonDataAsset(SkeletonDataAsset asset)
        {
            if (!asset)
            {
                return;
            }
            
            _placeHolderAnimation.skeletonDataAsset = asset;
        }
        public void SetAnimationActive(bool value)
        {
            _placeHolderAnimation.gameObject.SetActive(value);
        }
    }
}