using System;
using BugsFarm.Services.UIService;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIFarmShopTabElement : MonoBehaviour
    {
        public event EventHandler<int> ClickEvent;
        [Header("Animation")]
        [SerializeField] private UIBaseAnimation _animation;
        
        [SerializeField] private RectTransform _rect;
        [SerializeField] private Button _button;

        
        private void Awake()
        {
            _button.onClick.AddListener(OnClickEvent);
            _animation.ResetValues();
        }

        public void Show()
        {
            _animation.Play();
        }

        public void Hide()
        {
            _animation.Backward();
        }
        
        private void OnClickEvent()
        {
            ClickEvent?.Invoke(this, _rect.GetSiblingIndex());
        }
        
        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnClickEvent);
            ClickEvent = null;
        }
    }
}