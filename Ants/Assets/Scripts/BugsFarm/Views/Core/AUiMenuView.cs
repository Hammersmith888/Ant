namespace BugsFarm.Views.Core
{
    public abstract class AUiMenuView : AUiView
    {
        public virtual void Open()
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
        }

        public void Close()
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }
    }
}