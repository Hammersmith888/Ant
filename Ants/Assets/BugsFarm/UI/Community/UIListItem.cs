using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIListItem : MonoBehaviour
    {
        public event EventHandler OnClick;
        [SerializeField] private Image _image;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Button _button;

        public void SetImage(Sprite sprite)
        {
            _image.sprite = sprite;
        }
        public void SetText(string level)
        {
            _text.text = level;
        }
        public void SetInteractable(bool interactable)
        {
            _button.interactable = interactable;
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnClicked);
        }
        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnClicked);
        }
        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnClicked);
        }
        private void OnClicked()
        {
            OnClick?.Invoke(this,EventArgs.Empty);
        }
    }
}