using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace BugsFarm.Services.UIService
{
    public abstract class UISimpleWindow : UIWindow
    {
        [SerializeField] protected UIBaseAnimation animation;
        [SerializeField] private UnityEvent _onShow;
        [SerializeField] private UnityEvent _onHide;
        public event EventHandler CloseEvent;
        


        protected virtual void Awake()
        {
            gameObject.SetActive(false);

            if (animation != null)
            {
                animation.ResetValues();
            }
        }

        public override void Show()
        {
            if (animation != null)
                animation.ResetValues();
            
            gameObject.SetActive(true);

            if (animation != null)
                animation.Play();

            DOVirtual.DelayedCall(0.5f, () => _onShow?.Invoke()); // for VeilCloseButton need pause
        }

        public override void Hide()
        {
            if (animation != null)
            {
                animation.Backward(OnHided);
            }
            else
            {
                OnHided();
            }
            _onHide?.Invoke();
            CloseEvent = null;
        }

        public virtual void Close()
        {
            CloseEvent?.Invoke(this, EventArgs.Empty);
            CloseEvent = null;
        }

        protected override void OnHided()
        {
            gameObject.SetActive(false);
            base.OnHided();
        }
    }
}