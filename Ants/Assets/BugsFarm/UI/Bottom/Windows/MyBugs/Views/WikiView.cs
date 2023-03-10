using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class WikiView : BaseView
    {
        [SerializeField] private TextMeshProUGUI _headerText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _buttonAcceptText;
        [SerializeField] private Button _acceptButton;
        
        public event EventHandler AcceptEvent;

        public override void Show()
        {
            base.Show();
            _acceptButton.onClick.AddListener(OnAcceptEventHandler);
        }
        
        public override void Hide()
        {
            base.Hide();
            _acceptButton.onClick.RemoveListener(OnAcceptEventHandler);
            
            _headerText.text = string.Empty;
            _descriptionText.text = string.Empty;
            _buttonAcceptText.text = string.Empty;
            AcceptEvent = null;
        }
        
        public void SetHeader(string header)
        {
            _headerText.text = header;
        }

        public void SetDescription(string text)
        {
            _descriptionText.text = text;
        }

        public void SetAcceptText(string text)
        {
            _buttonAcceptText.text = text;
        }

        private void OnAcceptEventHandler()
        {
            AcceptEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}