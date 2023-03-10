using UnityEngine;


public abstract class APanel : MonoBehaviour
{
    public PanelID ID { get; private set; }
    public bool IsModal => _isModal;

    private bool _isModal;
    private bool _manualClose;

    public void Init(PanelID panelId)
    {
        ID = panelId;

        Init(out _isModal, out _manualClose);
    }

    public void Open()
    {
        gameObject.SetActive(true);
        OnOpened();
        //UI_Control.Instance?.cb_Opened(this);
    }

    public virtual void Close()
    {
        if (!_manualClose)
            gameObject.SetActive(false);

        OnClosed();

        //if (!_manualClose)
            //UI_Control.Instance?.cb_Closed(this);
    }

    protected abstract void Init(out bool isModal, out bool manualClose);

    protected virtual void UnInit()
    {
    }

    protected virtual void OnOpened()
    {
    }

    protected virtual void OnClosed()
    {
    }
}