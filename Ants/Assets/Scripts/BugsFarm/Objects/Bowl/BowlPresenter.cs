using System;
using BugsFarm;
using BugsFarm.AudioSystem;
using BugsFarm.AudioSystem.Obsolete;
using BugsFarm.Config;

[Serializable]
public class BowlPresenter : AConsumable
{
    [field: NonSerialized] public MB_Bowl MbBowl { get; private set; }
    private Timer _timer = new Timer();
    protected override A_MB_Consumable MbConsumable => MbBowl;

    public BowlPresenter(int placeNum) : base(ObjType.Bowl,
                                     0,
                                     placeNum,
                                     Data_Objects.Instance.GetData(ObjType.Bowl).upgrades.levels[0].param1)
    { }
    public override void Init(A_MB_Placeable mbBowl)
    {
        MbBowl = (MB_Bowl)mbBowl;
        InitOccupyable();
    }
    public override void PostSpawnInit()
    {
        base.PostSpawnInit();
        SetSprite();
        SetTimer();
    }
    public override void PostLoadRestore()
    {
        base.PostLoadRestore();
        SetSprite();
    }
    public override void Upgrade()
    {
        base.Upgrade();

        QuantityMax = UpgradeLevelCur.param1;

        SetSprite();
    }
    public override void Update()
    {
        base.Update();

        if (
                UpgradeLevelCur.param2 > 0 &&
                UpgradeLevelCur.param3 > 0 &&
                _timer.IsReady
            )
        {
            AddWater(UpgradeLevelCur.param2);

            SetTimer();
        }
    }
    public void cb_Tapped()
    {
        // if (
        //         UpgradeLevelCur.param2 <= 0 &&                                      // Config_A
        //         (
        //             QuantityCur < QuantityMax - Constants.Water_TAPable ||          // Early to open
        //             //UI_Control.Instance.IsOpened(PanelID.HudInfoPanel)               // Already opened
        //         )
        //     )
        //     AddWaterTap();
        // else
        //     GameEvents.OnObjectTap?.Invoke(MbBowl);
    }
    public void cb_TappedSpecial()
    {
        if (UpgradeLevelCur.param2 <= 0)
        {
            AddWaterTap();
        }
    }

    protected override void SetSprite()
    {
        MbBowl.SetWaterSprite(QuantityCur / QuantityMax);
    }
    protected override void OnDepleted()
    {
        base.OnDepleted();

        Analytics.NoResources(true);
    }
    private void AddWaterTap() => AddWater(UpgradeLevelCur.param1 / 5);
    private void AddWater(float quantity)
    {
        if (QuantityCur < QuantityMax)
            Sounds.Play(Sound.Water);

        Add(quantity);
    }
    private void SetTimer() => _timer.Set(UpgradeLevelCur.param3 * 60);
}

