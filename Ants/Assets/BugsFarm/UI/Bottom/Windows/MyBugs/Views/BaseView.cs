using UnityEngine;

namespace BugsFarm.UI
{
    public abstract class BaseView : MonoBehaviour
    {
        protected virtual void Awake()
        {
            gameObject.SetActive(false);
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }
        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}