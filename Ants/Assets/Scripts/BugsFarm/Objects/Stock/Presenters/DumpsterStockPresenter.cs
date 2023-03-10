using System;
using UnityEngine;

[Serializable]
public class DumpsterStockPresenter : FoodStockPresenter
{
    public override bool IsGarbage => true;
    private Timer _timer = new Timer();

    public DumpsterStockPresenter(int placeNum) : base(FoodType.DumpsterStock, placeNum, false)
    {
        QuantityMax = UpgradeLevelCur.param1;
    }
    public override void Update()
    {
        base.Update();

        if (_timer.IsReady)
        {
            var able = UpgradeLevelCur.param2;
            var have = ((AConsumable) this).QuantityCur;
            var recycled = Mathf.Min(able, have);

            base.Add(able * (-1));              // Recycle garbage

            SetTimer();

            Keeper.Stats.cb_GarbageRecycled(recycled);
        }
    }
    public override float Add(float quantity)
    {
        if ( quantity > 0 && ((AConsumable) this).QuantityCur <= 0 )
            SetTimer();

        return base.Add(quantity);
    }

    protected override void OnDepleted()
    {
        // Do nothing.
    }
    private void SetTimer()
    {
        _timer.Set(Constants.DumpsterRecycleTime);
    }
}

