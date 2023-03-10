using System;
using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIMessageBoxWindow : UISimpleWindow
    {
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private TextMeshProUGUI _acceptText;
        [SerializeField] private Button _buttonAccept;
        
        private const string _deafultAcceptText = "OK";
        
        public event EventHandler AcceptEvent;

        public override void Show()
        {
            base.Show();

            _buttonAccept.onClick.AddListener(AcceptEventHandler);
        }
        
        public override void Hide()
        {
            base.Hide();

            _buttonAccept.onClick.RemoveListener(AcceptEventHandler);

            AcceptEvent = null;
        }
        
        public void SetAcceptText(string text)
        {
            if (!_acceptText)
            {
                Debug.LogError($"{this} : TextMeshProUGUI _acceptText does not exist");
                return;
            }
            _acceptText.text = text;
        }

        public void SetMessageText(string text)
        {
            if (!_messageText)
            {
                Debug.LogError($"{this} : TextMeshProUGUI _messageText does not exist");
                return;
            }
            _messageText.text = text;
        }

        private void AcceptEventHandler()
        {
            AcceptEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}