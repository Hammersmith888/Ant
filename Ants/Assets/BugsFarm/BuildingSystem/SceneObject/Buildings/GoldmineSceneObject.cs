using UnityEngine;

namespace BugsFarm.BuildingSystem
{
    public class GoldmineSceneObject : BuildingSpineObject
    {
        public Transform PopupPoint => _popupPoint;
        [SerializeField] private SpriteRenderer _rocks;
        [SerializeField] private Transform _popupPoint;
        public override void SetPlace(APlace place)
        {
            base.SetPlace(place);
            var goldminePlace = (GoldminePlace) place;
            var rockTransform = goldminePlace.Rocks.transform;
            _rocks.transform.position = rockTransform.position;
            _rocks.transform.rotation = rockTransform.rotation;
            _rocks.transform.localScale = rockTransform.localScale;
            
            _rocks.sortingLayerID = goldminePlace.Rocks.sortingLayerID;
            _rocks.sortingOrder = goldminePlace.Rocks.sortingOrder;
        }
        
        public override void SetAlpha(float alpha01)
        {
            var rockColor = _rocks.color;
            rockColor.a = alpha01;
            _rocks.color = rockColor;
            base.SetAlpha(alpha01);
        }
        public override void SetObject(params object[] args) { }

        public void SetActiveRock(bool active)
        {
            if (_rocks == null)
                return;
            _rocks.gameObject.SetActive(active);
        }
    }
}