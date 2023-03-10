using UnityEngine;
using UnityEngine.U2D;

namespace BugsFarm.RoomSystem
{
    [RequireComponent(typeof(Collider2D))]
    public class RoomVineOpenableSceneObject : RoomOpeanbleSceneObject
    {
        private SpriteShapeRenderer ShapeRenderer => _graphicObject.GetComponent<SpriteShapeRenderer>();
        public override void ChangeAlpha(float alpha01)
        {
            base.ChangeAlpha(alpha01);
            var shapeRenderer = ShapeRenderer;
            var color = shapeRenderer.color;
            color.a = alpha01;
            shapeRenderer.color = color;
        }
    }
}