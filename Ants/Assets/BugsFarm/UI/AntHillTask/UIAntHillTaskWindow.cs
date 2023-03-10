using System;
using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIAntHillTaskWindow : UISimpleWindow
    {
        public event Action OnWindowClose;

        public UIAntHillTaskSlot AntHillTaskSlot => slotPrefab;
        public RectTransform SlotParentRectTransform => slotParentRectTransform;
        
        [SerializeField] private UIAntHillTaskSlot slotPrefab;
        [SerializeField] private RectTransform slotParentRectTransform;
        [SerializeField] private TextMeshProUGUI taskWindowTitle;
        [SerializeField] private Button closeButton;

        public void SetTitle(string titleText)
        {
            taskWindowTitle.text = titleText;
        }
        
        public override void Show()
        {
            base.Show();
            
            closeButton.onClick.AddListener(CloseWindow);
        }

        public override void Hide()
        {
            base.Hide();
            
            closeButton.onClick.RemoveListener(CloseWindow);
        }
        private void CloseWindow()
        {
            OnWindowClose?.Invoke();
        }
        
    }
}