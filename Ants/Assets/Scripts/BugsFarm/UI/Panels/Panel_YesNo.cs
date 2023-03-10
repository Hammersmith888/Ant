using System;
using System.Collections;
using BugsFarm.Game;
using UnityEngine;
using UnityEngine.UI;


public class Panel_YesNo : APanel
{
    public static bool IsYes;

    public static Panel_YesNo Instance { get; private set; }


#pragma warning disable 0649

    [SerializeField] Text _text_Header;
    [SerializeField] Text _text_Description;
    [SerializeField] Text _text_Price;

    [SerializeField] GameObject _priceAndCCY;

    public Button _button_Yes;
    public Button _button_No;
    public Button _button_Close;

#pragma warning restore 0649


    Animation _animation;
    CanvasGroup _canvasGroup;

    bool _closed;

    int? _price;
    Action _action;


    public IEnumerator WaitUntilClosed()
    {
        _closed = false;

        yield return new WaitUntil(() => _closed);
    }


    protected override void Init(out bool isModal, out bool manualClose)
    {
        isModal = true;
        manualClose = true;

        _animation = GetComponent<Animation>();
        _canvasGroup = GetComponent<CanvasGroup>();

        Instance = Tools.SingletonPattern(this, Instance);

        GameTools.DoubleSpeed(_animation);

        GameResources.OnCrystalsChanged += SetInteractable;
    }


    public void OpenDialog(string header, string description, int? price = null, Action action = null)
    {
        _price = price;
        _action = action;

        _text_Header.text = header;
        _text_Description.text = description;
        _text_Price.text = price.ToString();

        _priceAndCCY.SetActive(price.HasValue && price > 0);
        SetInteractable();

        //UI_Control.Instance.Open(PanelID.PopupYesNoIap);
    }


    void SetInteractable()
    {
        _button_Yes.interactable = !_price.HasValue || _price <= GameResources.Crystals;
    }


    public void OnYes()
    {
        IsYes = true;

        if (_price.HasValue)
            GameResources.Crystals -= _price.Value;

        _action?.Invoke();

        Close();
    }


    protected override void OnOpened()
    {
        _animation.Play("popup");

        IsYes = false;
    }


    protected override void OnClosed()
    {
        _canvasGroup.blocksRaycasts = false;

        _animation.Play("popout");
    }


    public void EventOpened()
    {
        _canvasGroup.blocksRaycasts = true;

        GameEvents.PanelOpenAnimDone?.Invoke(); // for Tutorial
    }


    public void EventClosed()
    {
        gameObject.SetActive(false);
        //UI_Control.Instance.cb_Closed(this);

        _closed = true;
    }
}