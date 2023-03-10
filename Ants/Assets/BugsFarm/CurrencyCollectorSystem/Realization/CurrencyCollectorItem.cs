using BugsFarm.Services.MonoPoolService;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace BugsFarm.CurrencyCollectorSystem
{
    public class CurrencyCollectorItem : MonoBehaviour, IMonoPoolable, ICurrencyAnimationItem
    {
        public GameObject GameObject => _selfContainer.gameObject;
        public RectTransform ParentAnimateTarget => _selfContainer;
        public RectTransform ChildAnimateTarget => _currencyIco.rectTransform;
        [SerializeField] private Image _currencyIco;
        [SerializeField] private RectTransform _selfContainer;

        private IMonoPool _monoPool;
        [Inject]
        private void Inject(IMonoPool monoPool)
        {
            _monoPool = monoPool;
        }
        public void SetSprite(Sprite sprite)
        {
            if (_currencyIco && sprite)
            {
                _currencyIco.sprite = sprite;
            }
        }

        public void OnDespawned()
        {
            if (_currencyIco)
            {
                ParentAnimateTarget.position   = ChildAnimateTarget.localPosition = Vector3.zero;
                ParentAnimateTarget.rotation   = ChildAnimateTarget.localRotation = Quaternion.identity;
                ParentAnimateTarget.localScale = ChildAnimateTarget.localScale    = Vector3.one;
                GameObject.SetActive(false);
            }
        }

        public void OnSpawned()
        {
            if (_currencyIco)
            {
                GameObject.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            _monoPool?.Destroy(this);
        }

        private void OnValidate()
        {
            if (!_selfContainer)
            {
                _selfContainer = GetComponent<RectTransform>();
            }
        }
    }
}