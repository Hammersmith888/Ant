using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIHintWindow : UISimpleWindow
    {
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private TextMeshProUGUI _headerText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Button[] _closeButtons;
        
        public override void Show()
        {
            base.Show();

            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.AddListener(Close);
            }
        }
        public override void Hide()
        {
            base.Hide();
            
            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.RemoveListener(Close);
            }
        }
        
        public void SetHeader(string header)
        {
            _headerText.text = header;
        }
        
        public void SetHint(string message)
        {
            _messageText.text = message;
        }
        
        public void SetIcon(Sprite icon)
        {
            _iconImage.sprite = icon;
        }
    }
}
