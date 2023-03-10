using System;

[Serializable]
public class GardenPresenter : Food
{
    public bool IsRipe => _stage == 2;
    public bool WasGardenedRecently => !_timer_WasGardenedRecently.IsReady;
    public float TimeToGrow => _timer_Ripeness.Left;

    [NonSerialized] private MB_Garden _mb_Garden;

    private int _stage;
    private readonly Timer _timer_Ripeness = new Timer();
    private readonly Timer _timer_WasGardenedRecently = new Timer();

    public GardenPresenter(int placeNum) : base(FoodType.Garden, placeNum, true)
    {
        QuantityMax = UpgradeLevelCur.param1;

        Reset();
    }
    public override void Init(A_MB_Placeable mb_Garden)
    {
        base.Init(mb_Garden);

        _mb_Garden = (MB_Garden)mb_Garden;
    }
    public override void Update()
    {
        base.Update();

        switch (_stage)
        {
            case 0:
                if (_timer_Ripeness.Progress > .5f)
                {
                    _stage++;
                    SetSprite();
                }
                break;

            case 1:
                if (_timer_Ripeness.IsReady)
                {
                    GrowthComplete();
                    SetSprite();
                }
                break;
        }
    }

    public void SetTimer()
    {
        _timer_WasGardenedRecently.Set(60 * 30);        // 30 min
    }
    private void Reset()
    {
        _stage = 0;
        QuantityCur = 0;

        _timer_Ripeness.Set(UpgradeLevelCur.param2 * 60);
    }
    private void GrowthComplete()
    {
        _stage = 2;
        QuantityCur = UpgradeLevelCur.param1;
    }

    protected override void CastObjectEvent(ObjEvent objEvent)
    {
        base.CastObjectEvent(objEvent);

        switch (objEvent)
        {
            case ObjEvent.BuildUpgradeBgn: _timer_Ripeness.Pause(); break;
            case ObjEvent.BuildUpgradeEnd: _timer_Ripeness.Unpause(); break;
        }
    }
    protected override void OnDepleted()
    {
        base.OnDepleted();

        Reset();
        SetSprite();
    }
    protected override void SetSprite()
    {
        _mb_Garden.SetSprite(_stage);
    }
}