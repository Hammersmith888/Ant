using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class MyBugItem : MonoBehaviour
    {
        public string Id { get; set; }
        [SerializeField] private Button _infoButton;
        [SerializeField] private Image _iconPlaceHolder;
        [SerializeField] private Outline _selection;
        [SerializeField] private TextMeshProUGUI _levelText;
        public event EventHandler<string> ClickEvent;

        
        private void Awake()
        {
            _infoButton.onClick.AddListener(OnInfoEventHandler);
            SetSelected(false);
        }

        public void SetLevelText(string value)
        {
            _levelText.text = value;
        }
        
        public void SetSelected(bool value)
        {
            _selection.enabled = value;
        }

        public void SetIcon(Sprite value)
        {
            _iconPlaceHolder.sprite = value;
        }
        
        private void OnDestroy()
        {
            _infoButton.onClick.RemoveListener(OnInfoEventHandler);
            ClickEvent = null;
        }

        private void OnInfoEventHandler()
        {
            ClickEvent?.Invoke(this, Id);
        }
    }
}