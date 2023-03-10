using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UITaskStorageDebugWindow : UISimpleWindow
    {
        public event UnityAction OnClose
        {
            add => _buttonClose.onClick.AddListener(value);
            remove => _buttonClose.onClick.RemoveListener(value);
        }
        
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Button _buttonClose;
  
        
        public void SetText(string message)
        {
            _messageText.text = message;
        }
    }
}
