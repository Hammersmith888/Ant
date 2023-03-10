using BugsFarm.Services.StorageService;
using UnityEngine;
using Zenject;

namespace BugsFarm.RoomSystem
{

    public class RoomBaseSceneObject : MonoBehaviour, IStorageItem
    {
        public string Id { get; private set; }
        public GameObject SelfContainer => _selfContainer;
        [SerializeField] private GameObject _selfContainer;
        [Inject]
        private void Inject(string id)
        {
            Id = id;
        }
        
        public virtual void ChangeVisible(bool value){}
        public virtual void ChangeIntreaction(bool value){}
        public virtual void ChangeAlpha(float alpha01){}

        protected virtual void OnValidate()
        {
            if (!_selfContainer)
            {
                _selfContainer = gameObject;
            }
        }
    }
}