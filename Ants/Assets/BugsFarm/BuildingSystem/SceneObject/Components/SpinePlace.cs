using Spine.Unity;
using UnityEngine;

namespace BugsFarm.BuildingSystem
{
    public class SpinePlace : APlace
    {
    #pragma warning disable 0649
        [SerializeField] private SkeletonAnimation _spine;
    #pragma warning restore 0649

        private void Start()
        {
            _spine.Initialize(false);           // Required because was disabled before and not yet initialized (which is done in Awake())
            _spine.skeleton.A = .5f;
        }
    }
}

