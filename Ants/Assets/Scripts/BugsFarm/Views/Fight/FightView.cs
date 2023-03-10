using BugsFarm.Views.Core;
using UnityEngine;

namespace BugsFarm.Views.Fight
{
    public class FightView : AView
    {
        [SerializeField] private SpriteRenderer veilTopSide;
        [SerializeField] private Transform ground;
        [SerializeField] private Transform parentUnits;
        [SerializeField] private Transform parentArrows;
        public Camera MainCamera;

        public SpriteRenderer VeilTopSide => veilTopSide;
        public Transform Ground => ground;
        public Transform ParentUnits => parentUnits;
        public Transform ParentArrows => parentArrows;
    }
}