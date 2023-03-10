using System.Linq;
using BugsFarm.CurrencyCollectorSystem;

using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class QuestItem : MonoBehaviour
{

    [SerializeField] private Image _icon;
    [SerializeField] private Text _description;
    [SerializeField] private Text _reward;
    [SerializeField] private Image _progressBar;
    [SerializeField] private Text _progressText;
    [SerializeField] private Button _buttonGetReward;

    private const float _tweenDelay = .125f;
    private const float _tweenDuration = .5f;
    private const float _tweenAdvance = .25f;
    
    private Quest Quest => Keeper.Quests[_config.id];
    private CfgQuest _config;
    private RectTransform _rt;
    private RectTransform _parent;

    private int _coins;
    private ICurrencyCollectorSystem _currencyCollection;
    [Inject]
    private void Inject(ICurrencyCollectorSystem currencyCollection, CfgQuest config)
    {
        _currencyCollection = currencyCollection;
        Init(config);
    }
    
    public void Refresh()
    {
        var quest = Quest;
        
        _progressBar.fillAmount = (float) quest.CompletedStages / _config.stages;
        _progressText.text = quest.Completed ? Texts.QuestComplete : $"{quest.CompletedStages} / {_config.stages}";

        _buttonGetReward.gameObject.SetActive(quest.Status == QuestStatus.RewardNotTaken);
    }
    public void OnGet()
    {
        const string currencyIdCouins = "0";
        // TODO : Action<int> onLeftCounts - OnCoinDec теперь этот калбек возвращает остаток сбора, тобишь только визуальное оформление.
        _currencyCollection.Collect(
            _buttonGetReward.transform.position, 
            currencyIdCouins, 
            _config.reward,
            false,
            OnCoinDec,
            Tween);

        Quest.RewardTaken();
        Refresh();

        // Analytics
        {
            BugsFarm.Analytics.TaskComplete(_config.id);

            if (_config.id != QuestID.Recycle500 && !Keeper.Quests.Any(x =>
                                       x.Value.Status != QuestStatus.RewardTaken &&
                                       x.Value.ID != QuestID.Recycle500))
            {
                BugsFarm.Analytics.AllTasksDone();
            }
        }
    }

    private void Init(CfgQuest config)
    {
        _config = config;


        _icon.sprite = config.icon;
        _icon.rectTransform.sizeDelta = new Vector2(config.icon.texture.width, config.icon.texture.height);

        _description.text = config.description;

        _coins = config.reward;
        _reward.text = config.reward.ToString();

        _rt = GetComponent<RectTransform>();
        _parent = transform.parent.GetComponent<RectTransform>();
    }
    private void OnCoinDec(int value)
    {
        _coins -= value;
        _reward.text = _coins.ToString();

        if (_coins == 0)
        {
            _reward.transform.parent.gameObject.SetActive(false);
        }
    }
    private void Tween()
    {
        CreateDummy();

        /*
            So its position not controlled by the VerticalGroupLayout
            Important in case there are more than 1 active tween
        */
        transform.SetParent(transform.parent.parent);

        transform
            .DOMoveX(Tools.CamSize().x * (-.5f), _tweenDuration)
            .SetDelay(_tweenDelay)
            .SetEase(Ease.InCubic)
            .OnComplete(() => { });// menu_Quests.Instance.DeleteItem(this)
    }
    private void CreateDummy()
    {
        RectTransform rt = Tools.CopyRectTransform(_rt);

        rt.transform.SetSiblingIndex(transform.GetSiblingIndex());

        rt
            .DOSizeDelta(Vector2.zero, .5f)
            .SetDelay(_tweenDelay + _tweenDuration - _tweenAdvance)
            .SetEase(Ease.InCubic)
            .OnUpdate(() => LayoutRebuilder.ForceRebuildLayoutImmediate(_parent))
            .OnComplete(() => Destroy(rt.gameObject))
            ;
    }
}