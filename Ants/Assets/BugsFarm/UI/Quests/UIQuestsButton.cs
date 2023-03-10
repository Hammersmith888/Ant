using System;
using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    [RequireComponent(typeof(UIBaseAnimation))]
    public class UIQuestsButton : UISimpleWindow
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _questsCountText;
        [SerializeField] private GameObject _questsCountContainer;
        
        public event EventHandler ClickEvent;
        
        public override void Show()
        {
            base.Show();
            
            _button.onClick.AddListener(ClickEventHandler);
        }

        public override void Hide()
        {
            base.Hide();
            
            _button.onClick.RemoveListener(ClickEventHandler);
            
            ClickEvent = null;
        }
        
        public void ChangeQuestCountText(string text)
        {
            if (!_questsCountText)
            {
                Debug.LogError($"{nameof(ChangeQuestCountText)} :: {nameof(_questsCountText)} is Null ", this);
                return;
            }
            _questsCountText.text = text;
        }
        
        public void ChangeQuestCountActive(bool active)
        {
            if (!_questsCountContainer)
            {
                Debug.LogError($"{nameof(ChangeQuestCountActive)} :: {nameof(_questsCountContainer)} is Null ", this);
                return;
            }
            _questsCountContainer.SetActive(active);
        }
  
        private void ClickEventHandler()
        {
            ClickEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}