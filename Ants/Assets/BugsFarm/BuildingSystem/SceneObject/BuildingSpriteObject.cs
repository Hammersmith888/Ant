using BugsFarm.Graphic;
using UnityEngine;

namespace BugsFarm.BuildingSystem
{
    public class BuildingSpriteObject : BuildingSceneObject
    {
        private SpriteRenderer SpriteRenerer => (SpriteRenderer)_mainRenerer;
        
        public override void SetObject(params object[] args)
        {
            if(!SpriteRenerer) return;
            SpriteRenerer.sprite = (Sprite) args[0];
        }
        
        public override void SetLayer(LocationLayer layer)
        {
            base.SetLayer(layer);
            if(!SpriteRenerer) return;
            SpriteRenerer.sortingLayerID = layer.ID;
            SpriteRenerer.sortingOrder = layer.Order;
        }
        
        public override void SetAlpha(float alpha01)
        {
            if(!SpriteRenerer) return;
            var sourceColor = SpriteRenerer.color;
            sourceColor.a = alpha01;
            SpriteRenerer.color = sourceColor;
        }
    }
}