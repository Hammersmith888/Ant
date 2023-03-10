using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class ShopItem : MonoBehaviour
{
    public Image itemBG;
    public Image itemIcon;
    public Image coin;
    public Text priceText;
    public Button buyButton;
    public Button infoButton;

    public ObjType type;
    public int subType;
    public int price;
    public int maxCount;

    public Text text_Locked;

    private CfgObject _data;

    public void Setup(CfgObject data, int subType)
    {
        _data = data;

        type = data.type;
        this.subType = subType;
        maxCount = data.maxCount;

        if (data.isLocked)
        {
            itemBG.sprite = data.wiki.Icon;
            text_Locked.text = string.Format(Texts.UnlocksAfter, data.unlocksAfter);
        }
        else
        {
            itemIcon.sprite = data.wiki.Icon;
        }

        itemIcon.gameObject.SetActive(!data.isLocked);
        buyButton.gameObject.SetActive(!data.isLocked);
        text_Locked.gameObject.SetActive(data.isLocked);

        SetPrice();

        infoButton.onClick.AddListener(() => Panel_FarmMenu.Instance.OnPrepareBuy(data, subType));
        buyButton.onClick.AddListener(() => Panel_FarmMenu.Instance.OnBuy(data.type, subType));
    }
    public void Refresh()
    {
        SetPrice();
        SetInteractable();
    }

    private void SetPrice()
    {
        int price = FirstHour.GetPrice(_data, subType);
        bool isFree = price == 0;

        this.priceText.text = isFree ? Texts.Free : price.ToString();
        this.price = price;

        coin.gameObject.SetActive(!isFree);
    }
    private void SetInteractable()
    {
        bool interactable =
                                        price <= GameResources.Coins &&
                                        (
                                            maxCount == 0 ||
                                            GetCount() < maxCount
                                        )
        ;

        buyButton.interactable = interactable;
        infoButton.interactable = interactable && !_data.isLocked;
        coin.color = interactable ? Color.white : buyButton.colors.disabledColor;
    }
    private int GetCount()
    {
        bool Predicate(APlaceable x) =>
                                                x.SubType == subType &&
                                                (
                                                    type != ObjType.Food ||
                                                    !((Food)x).IsGarbage
                                                )
        ;

        return Keeper.GetObjects(type).Count(Predicate);
    }
}

