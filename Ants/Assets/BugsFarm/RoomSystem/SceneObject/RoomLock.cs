using BugsFarm.Services.MonoPoolService;
using UnityEngine;
using Zenject;

namespace BugsFarm.RoomSystem
{
    public class RoomLock : MonoBehaviour, IMonoPoolable
    {
        public GameObject GameObject => gameObject;
        private IMonoPool _monoPool;

        [Inject]
        private void Inject(IMonoPool monoPool)
        {
            _monoPool = monoPool;
        }

        public void SetPosition(Vector2 position)
        {
            transform.position = position;
        }

        public void OnDespawned()
        {
            gameObject.SetActive(false);
        }

        public void OnSpawned()
        {
            gameObject.SetActive(true);
        }

        private void OnDestroy()
        {
            _monoPool?.Destroy(this);
        }
    }
}