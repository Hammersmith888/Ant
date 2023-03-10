using System.Collections.Generic;
using BugsFarm;
using UnityEngine;
using UnityEngine.UI;


public class Panel_Shop : APanel
{
#pragma warning disable 0649

    [SerializeField] List<Text> _amounts;
    [SerializeField] List<Text> _prices;

    [SerializeField] YesNo_IAP _yesNo;

    [SerializeField] Button _button_Coins_1;
    [SerializeField] Button _button_Coins_2;
    [SerializeField] Button _button_Coins_3;

    [SerializeField] Transform _guide_ShopCenter;

#pragma warning restore 0649


    protected override void Init(out bool isModal, out bool manualClose)
    {
        isModal = true;
        manualClose = false;


        for (int i = 0; i < 6; i++)
            SetText((IAPType) (i + 1), _amounts[i], _prices[i]);

        GameResources.OnCrystalsChanged += SetInteractable;


        // Fix for "not-very-tall" resolutions
        if (_guide_ShopCenter.position.y > 0)
            _guide_ShopCenter.parent.position -= Vector3.up * _guide_ShopCenter.position.y;
    }


    public static void SetText(IAPType type, Text amount, Text price)
    {
        FB_CfgIAP data = Data_IAPs.Instance.IAPs[type].IAP;

        amount.text = Tools.ThSep(data.amount);

        // price.text			= data.price + (type < IAPType.Crystals_1 ? "" : " $");
        price.text =
            (
                type >= IAPType.Crystals_1 &&
                type <= IAPType.Crystals_3
            )
                ? (
                    Purchaser.IsInitialized() ? Purchaser.GetLocalPriceString(type) : data.price + " $"
                )
                : Mathf.RoundToInt(data.price).ToString()
            ;
    }


    protected override void OnOpened()
    {
        SetInteractable();
        // UI_Control.Instance.SetVeil( 1, true );
    }


    protected override void OnClosed()
    {
        // UI_Control.Instance.SetVeil( 1, false );
    }


    void SetInteractable()
    {
        _button_Coins_1.interactable = GameResources.Crystals >= Data_IAPs.Instance.IAPs[IAPType.Coins_1].IAP.price;
        _button_Coins_2.interactable = GameResources.Crystals >= Data_IAPs.Instance.IAPs[IAPType.Coins_2].IAP.price;
        _button_Coins_3.interactable = GameResources.Crystals >= Data_IAPs.Instance.IAPs[IAPType.Coins_3].IAP.price;
    }


    [EnumAction(typeof(IAPType))]
    public void OnBuy(int type)
    {
        _yesNo.Open((IAPType) type);
    }
}