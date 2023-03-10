using System.Linq;
using BugsFarm.Game;
using BugsFarm.SimulationSystem;
using BugsFarm.SimulationSystem.Obsolete;
using BugsFarm.UnitSystem.Obsolete;
using UnityEngine;
using UnityEngine.UI;

public class BugInfo : MonoBehaviour
{
    public static BugInfo Instance { get; private set; }
    
    [SerializeField] private Text _header;
    [SerializeField] private Image _icon;
    [SerializeField] private Image _levelProgress;
    [SerializeField] private Transform _FX;
    [SerializeField] private Button _buttonUpgrade;
    [SerializeField] private Text _buttonUpgradeCost;
    [SerializeField] private BugWiki _bugWiki;

    private AntPresenter _ant;
    private int _upgradeCost;

    public void Init()
    {
        Instance = Tools.SingletonPattern(this, Instance);
        _bugWiki.Init();
        Set(null);

        GameResources.OnCoinsChanged += SetInteractable;
        GameEvents.OnAntGotXP += Refresh;
        GameEvents.OnAntDied += (ant, reason) => Refresh();
        GameEvents.OnAntDestroyed += Refresh;
        GameEvents.OnGameReset += Refresh;
        GameEvents.OnSimulationEnd += Refresh;
    }

    private void Refresh(AntPresenter ant)
    {
        Refresh();
    }

    public void Refresh()
    {
        if (SimulationOld.Type != SimulationType.None)
            return;

        var noAnt = _ant == null || !Keeper.Ants.Contains(_ant) || !_ant.IsAlive;
        var ant = noAnt ? Keeper.Ants.ElementAtOrDefault(0) : _ant;
        Set(ant);
    }

    public void Set(AntPresenter ant)
    {
        _ant = ant;

        gameObject.SetActive(ant != null);

        if (ant == null)
            return;


        var t = (float) ant.LevelP1 / Constants.MaxUnitLevel;
        var isUpgradable = _ant.LevelP1 < Constants.MaxUnitLevel;
        var upgradeCost = Mathf.Max(Mathf.RoundToInt(_ant.XpRequiredToNextLevel() * _ant.CfgUnit.XPprice), 1);
        _upgradeCost = upgradeCost;

        _header.text = ant.Name;
        _icon.sprite = Data_Ants.Instance.GetData(ant.AntType).wiki.Icon;
        _levelProgress.fillAmount = t;

        Tools.MinMax_1RT(_levelProgress.rectTransform, out Vector2 min, out Vector2 max);

        _FX.position = _FX.position.SetX(Mathf.Lerp(min.x, max.x, t));

        _FX.gameObject.SetActive(isUpgradable);
        _buttonUpgrade.gameObject.SetActive(isUpgradable);
        _buttonUpgradeCost.text = Tools.ThSep(upgradeCost);

        SetInteractable();
    }

    private void SetInteractable()
    {
        _buttonUpgrade.interactable = GameResources.Coins >= _upgradeCost;
    }

    public void OnUpgrade()
    {
        GameResources.Coins -= _upgradeCost;

        _ant.Upgrade();

        Set(_ant);

        MyBugs.Instance.Setup();
    }

    public void OnInfo()
    {
        MyBugs.Instance.OpenClose(false);

        BugWiki.Instance.Open(_ant.AntType);
    }
}