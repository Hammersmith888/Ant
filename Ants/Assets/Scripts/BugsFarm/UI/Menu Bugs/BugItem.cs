using System.Linq;
using BugsFarm;
using BugsFarm.Game;
using UnityEngine;
using UnityEngine.UI;

public class BugItem : MonoBehaviour
{
    public AntType Type => _type;
    public Button button_Buy => _button_Buy;

    [SerializeField] private Text _text_Header;
    [SerializeField] private Text _text_Price;
    [SerializeField] private Image _icon;
    [SerializeField] private Image _icon_CCY;
    [SerializeField] private Button _button_Buy;

    [SerializeField] private GameObject _speed;
    [SerializeField] private GameObject _fightInfo;

    [SerializeField] private GameObject _buy;
    [SerializeField] private Text _locked;

    private CfgAnt _config;
    private AntType _type;
    private int _price;
    private Currency _currency;
    private MonoFactory _monoFactory;
    
    public void Init(CfgAnt config, MonoFactory monoFactory)
    {
        _config = config;
        _monoFactory = monoFactory;
        _type = config.antType;
        _currency = config.currency;

        _text_Header.text = config.wiki.Header;
        _icon.sprite = config.wiki.Icon;

        var currencySprite = Refs.Instance.GetCurrencySprite(config.currency);
        _icon_CCY.sprite = currencySprite;
        _icon_CCY.rectTransform.sizeDelta = new Vector2(currencySprite.texture.width, currencySprite.texture.height);

        var notQueen = config.antType != AntType.Queen;

        _speed.SetActive(notQueen);
        _fightInfo.SetActive(notQueen);

        _buy.SetActive(!config.isLocked);
        _locked.gameObject.SetActive(config.isLocked);
        _locked.text = string.Format(Texts.UnlocksAfter, config.unlocksAfter);

        SetPrice();
    }

    private void SetPrice()
    {
        var price = FirstHour.GetPrice(_config);
        var isFree = price == 0;

        _price = price;

        _text_Price.text = isFree ? Texts.Free : price.ToString();
        _text_Price.alignment = isFree ? TextAnchor.MiddleCenter : TextAnchor.MiddleRight;
        // _text_Price.color		= Refs.Instance.GetCurrencyColor( _config.currency );

        _icon_CCY.gameObject.SetActive(!isFree);
    }

    public void Refresh()
    {
        SetPrice();
        SetInteractable();
    }

    private void SetInteractable()
    {
        var resource = GameResources.GetResource(_currency);

        _button_Buy.interactable =
            resource >= _price &&
            (_type != AntType.Queen || !Keeper.GetObjects(ObjType.Queen).Any())
            ;
    }

    public void OnBuy()
    {
        if (_type == AntType.Queen)
        {
            Panel_FarmMenu.Instance.OnBuy(ObjType.Queen, 0);
        }
        else
        {
            GameResources.AddResource(_currency, _price * (-1));

            //_monoFactory.Spawn_Ant(null, _type);
        }

        GameEvents.OnAntTypeBought?.Invoke(_type);
        Analytics.InAppPurchase(_type, _price, _currency);

        Refresh(); // To update price. Price can be changed due to logic in FirstHour class.
    }

    public void OnHint()
    {
        Panel_Hint.Instance.OpenPanel(_type);
    }
}