using System;
using BugsFarm.Services.SimpleLocalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class OrderItem : MonoBehaviour
    {
        public event Action<string> OnCollectClick;
        public string Id { get; private set; }

        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _completeText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _resourceInfoText;
        [SerializeField] private TextMeshProUGUI _collectButtonText;
        [SerializeField] private Button _collectButton;

        private const string _compelteTextKey = "UIOrderBoard_OrderItemComplete";
        private const string _collectButtonTextKey = "UIOrderBoard_CollectOrder";
        private bool _initialized;

        public void Initilize(string id)
        {
            if (_initialized) return;
            Id = id;
            _collectButton.onClick.AddListener(OnCollectClicked);
            _initialized = true;
        }

        public void SetResourceInfo(string text)
        {
            if (!_resourceInfoText)
            {
                Debug.LogError($"{this} : TextMeshProUGUI _resourceInfoText is missing");
                return;
            }

            _resourceInfoText.text = text;
        }
        
        public void SetInteractableCollect(bool interactable)
        {
            if (!_collectButton)
            {
                Debug.LogError($"{this} : Button _collectButton is missing");
                return;
            }

            _collectButton.interactable = interactable;
        }

        public void SetIcon(Sprite sprite)
        {
            if (!_icon || !sprite)
            {
                Debug.LogError($"{this} : Image _icon is missing");
                return;
            }

            _icon.sprite = sprite;
        }

        public void SetLevelText(string text)
        {
            if (!_levelText)
            {
                Debug.LogError($"{this} : TextMeshProUGUI _levelText is missing");
                return;
            }

            _levelText.text = text ?? "";
        }

        public void SetDescription(string text)
        {
            if (!_descriptionText) return;
            _descriptionText.text = text;
        }

        public void ActiveDescription(bool active)
        {
            if (!_descriptionText) return;
            _descriptionText.gameObject.SetActive(active);
        }

        public void ActiveResourceInfo(bool active)
        {
            if (!_resourceInfoText)
            {
                Debug.LogError($"{this} : TextMeshProUGUI _resourceInfoText is missing");
                return;
            }

            _resourceInfoText.gameObject.SetActive(active);
        }
        
        public void ActiveCollectButton(bool active)
        {
            if (!_collectButton)
            {
                Debug.LogError($"{this} : Button _collectButton is missing");
                return;
            }

            _collectButtonText.text = LocalizationManager.Localize(_collectButtonTextKey);
            _collectButton.gameObject.SetActive(active);
        }

        public void ActiveCompleteText(bool active)
        {
            if (!_completeText)
            {
                Debug.LogError($"{this} : TextMeshProUGUI _completeText is missing");
                return;
            }

            _completeText.text = LocalizationManager.Localize(_compelteTextKey);
            _completeText.gameObject.SetActive(active);
        }

        public void ActiveLevelImage(bool active)
        {
            if (!_levelText)
            {
                Debug.LogError($"{this} : TextMeshProUGUI _levelText is missing");
                return;
            }

            _levelText.transform.parent.gameObject.SetActive(active);
        }

        private void OnDestroy()
        {
            if (_collectButton)
            {
                _collectButton.onClick.RemoveListener(OnCollectClicked);
            }

            OnCollectClick = null;
        }

        private void OnCollectClicked()
        {
            OnCollectClick?.Invoke(Id);
        }
    }
}