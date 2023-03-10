using System;
using BugsFarm.Services.InputService;
using BugsFarm.Services.MonoPoolService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace BugsFarm.UI
{
    public class UIBubbleCurrency : MonoBehaviour, IMonoPoolable
    {
        public GameObject GameObject => gameObject;
        public event Action OnTapped;

        [SerializeField] private Button _tapButton;
        [SerializeField] private Image _currencyIco;
        [SerializeField] private TextMeshProUGUI _currencyText;
        [SerializeField] private TextMeshProUGUI _coinText;
        private IMonoPool _monoPool;
        private IInputController<MainLayer> _inputController;

        [Inject]
        private void Inject(IMonoPool monoPool,
                            IInputController<MainLayer> inputController)
        {
            _inputController = inputController;
            _monoPool = monoPool;
        }

        public void SetText(string text)
        {
            _currencyText.gameObject.SetActive(!string.IsNullOrEmpty(text));
            _currencyText.text = text;
        }

        public void ChangePosition(Vector3 worldSpace)
        {
            transform.position = worldSpace;
        }

        public void ChangeInteraction(bool interractable)
        {
            _tapButton.interactable = interractable;
        }
        
        public void SetIco(Sprite sprite)
        {
            _currencyIco.gameObject.SetActive(sprite);
            _currencyIco.sprite = sprite;
        }

        public void OnDespawned()
        {
            OnTapped = null;
            if (_tapButton)
            {
                _tapButton.interactable = false;
                _tapButton.onClick.RemoveListener(OnClick);
            }

            if (transform)
            {
                transform.position = Vector3.zero;
            }

            if (gameObject)
            {
                gameObject.SetActive(false);
            }

            if (_currencyText)
            {
                _currencyText.text = "";
                _currencyText.gameObject.SetActive(false);
            }

            if (_currencyIco)
            {
                _currencyIco.gameObject.SetActive(false);
                _currencyIco.sprite = null;
            }
        }

        public void OnSpawned()
        {
            if (_tapButton)
            {
                _tapButton.interactable = false;
                _tapButton.onClick.AddListener(OnClick);
            }

            if (gameObject)
            {
                gameObject.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            _monoPool?.Destroy(this);
        }

        private void OnClick()
        {
            if (_inputController.Locked) return;
            OnTapped?.Invoke();
        }
        internal void SetCoinValue(string coinAmount)
        {
            _coinText.text = coinAmount;
        }
    }
}