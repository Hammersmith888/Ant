using UnityEngine;
using UnityEngine.UI;


public class YesNo_IAP : MonoBehaviour
{
#pragma warning disable 0649

    [SerializeField] Text _amount;
    [SerializeField] Image _image;

    [SerializeField] Text _price;
    [SerializeField] Image _crystal;

    [SerializeField] Sprite[] _sprites;
    [SerializeField] RectTransform[] _refs;

#pragma warning restore 0649


    IAPType _type;


    public void Open(IAPType type)
    {
        _type = type;
        int i = (int) type - 1;

        _amount.rectTransform.anchoredPosition = _refs[i].anchoredPosition;

        Sprite sprite = _sprites[i];
        _image.sprite = sprite;
        _image.rectTransform.sizeDelta = new Vector2(sprite.texture.width, sprite.texture.height);

        Panel_Shop.SetText(type, _amount, _price);

        bool showCrystal = type >= IAPType.Coins_1 && type <= IAPType.Coins_3;
        _crystal.gameObject.SetActive(showCrystal);

        gameObject.SetActive(true);
    }


    void OnEnable()
    {
        //UI_Control.Instance.SetVeil(VeilType.Veil_2, true);
    }


    void OnDisable()
    {
        //UI_Control.Instance.SetVeil(VeilType.Veil_2, false);
    }


    public void OnBuy()
    {
        FB_CfgIAP cfg = Data_IAPs.Instance.IAPs[_type].IAP;

        switch (_type)
        {
            case IAPType.Coins_1:
            case IAPType.Coins_2:
            case IAPType.Coins_3:
                GameResources.Coins += cfg.amount;
                GameResources.Crystals -= (int) cfg.price;
                BugsFarm.Analytics.BuyCoins(_type);
                break;

            case IAPType.Crystals_1:
            case IAPType.Crystals_2:
            case IAPType.Crystals_3:
                BugsFarm.Purchaser.BuyCrystals(_type);
                break;
        }

        Close();
    }


    public void OnClose()
    {
        Close();
    }


    void Close()
    {
        gameObject.SetActive(false);
    }
}