using UnityEngine;

namespace BugsFarm.RoomSystem
{
    public class RoomAnimatedSceneObject : RoomBaseSceneObject
    {
        [SerializeField] private GameObject _openState;
        [SerializeField] private GameObject _closeState;
        public override void ChangeVisible(bool value)
        {
            _openState.SetActive(value);
            _closeState.SetActive(!value);
        }
    }
}