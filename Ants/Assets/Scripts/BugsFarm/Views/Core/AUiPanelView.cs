using BugsFarm.Tweens;
using UnityEngine;

namespace BugsFarm.Views.Core
{
    public abstract class AUiPanelView : AUiView
    {
        [SerializeField] protected Animation panelAnimation;
        [SerializeField] protected Tween tween;

        public Tween Tween => tween;

        public virtual void Open()
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
            
            if (panelAnimation)
                panelAnimation.Play();
            
            if (tween)
                tween.Do();
        }

        public virtual void Close()
        {
            if (tween)
            {
                tween.Rewind(
                    () => gameObject.SetActive(false));
            }
        }
    }
}