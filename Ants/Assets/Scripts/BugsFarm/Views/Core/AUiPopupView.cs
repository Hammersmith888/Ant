using UnityEngine;

namespace BugsFarm.Views.Core
{
    public abstract class AUiPopupView : AUiView
    {
        [SerializeField] protected Animation panelAnimation;

        public Animation PanelAnimation => panelAnimation;

        public virtual void Open()
        {
            if (panelAnimation)
                panelAnimation.Play();
        }
        
        public void Close()
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }
    }
}