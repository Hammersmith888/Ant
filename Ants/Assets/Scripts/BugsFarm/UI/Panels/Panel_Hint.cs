using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class Panel_Hint : APanel
{
    public static Panel_Hint Instance { get; private set; }


#pragma warning disable 0649

    [SerializeField] Text _text_Header;
    [SerializeField] TextMeshProUGUI _text_Descr;
    [SerializeField] Image _icon;

#pragma warning restore 0649


    protected override void Init(out bool isModal, out bool manualClose)
    {
        isModal = false;
        manualClose = false;

        Instance = Tools.SingletonPattern(this, Instance);
    }


    public void OpenPanel(AntType type)
    {
        Wiki wiki = Data_Ants.Instance.GetData(type).wiki;

        _text_Header.text = wiki.Header;
        _text_Descr.text = wiki.Description;
        _icon.sprite = wiki.Icon;

        //UI_Control.Instance.Open(PanelID.PopupHint);
    }
}