using UnityEngine;

namespace BugsFarm.BuildingSystem
{
    public class BowlSceneObject : BuildingSpriteObject
    {
        public SpriteRenderer WaterRenderer => _water;
        
        public float InitWaterSize { get; private set; }
        
        [SerializeField] private SpriteRenderer _water;

        private void Awake()
        {
            InitWaterSize = WaterRenderer.size.y;
        }

        public override void SetObject(params object[] args){}
    }
}