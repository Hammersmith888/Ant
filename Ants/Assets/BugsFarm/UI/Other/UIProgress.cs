using BugsFarm.Services.MonoPoolService;
using TMPro;
using UnityEngine;
using Zenject;

namespace BugsFarm.UI
{
    public class UIProgress : MonoBehaviour, IMonoPoolable
    {
        public GameObject GameObject => gameObject;

        [SerializeField] private SlicedFilledImage _fillImage;
        [SerializeField] private TextMeshProUGUI _textProgress;

        private IMonoPool _monoPool;
        
        [Inject]
        private void Inject(IMonoPool monoPool)
        {
            _monoPool = monoPool;
        }

        public void ChangePosition(Vector2 pos)
        {
            transform.position = pos;
        }

        public void ActiveText(bool active)
        {
            if (_textProgress)
            {
                _textProgress.gameObject.SetActive(active);
            }
        }

        public void ChangeText(string format)
        {
            if (_textProgress)
            {
                _textProgress.text = format;
            }
        }

        public void ChangeProgress(float progress01)
        {
            if (_fillImage)
            {
                _fillImage.fillAmount = progress01;
            }
        }

        public void OnDespawned()
        {
            if (_textProgress)
            {
                _textProgress.text = string.Empty;
            }

            if (_fillImage)
            {
                _fillImage.fillAmount = 0;
            }

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