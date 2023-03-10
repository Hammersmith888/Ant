using BugsFarm.Services.MonoPoolService;
using UnityEngine;

namespace BugsFarm.AudioSystem
{
    // TODO rename to MusicPlayer
    public class AudioPlayer : MonoBehaviour, IMonoPoolable
    {
        public GameObject GameObject => gameObject;
        public AudioSource Player => _audioSource;
        [SerializeField] private AudioSource _audioSource;

        public void OnDespawned()
        {
            _audioSource.clip = null;
            _audioSource.volume = 1;
            _audioSource.mute = false;
            transform.position = Vector3.zero;
            transform.localScale = Vector3.one;
            transform.rotation = Quaternion.identity;
            _audioSource.minDistance = 7;
            _audioSource.maxDistance = 8;
            _audioSource.spatialBlend = 1;
            gameObject.SetActive(false);
        }

        public void OnSpawned()
        {
            gameObject.SetActive(true);
        }
    }
}