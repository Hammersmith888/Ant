using System;
using BugsFarm.Game;

[Serializable]
public class FightStockPresenter : FoodStockPresenter
{
    public bool IsFightStockVisible => _isFightStockVisible;
    protected override bool IsInvisibleWhenEmpty => false;
    private bool IsFightStock => SubType == (int)FoodType.FightStock;

    // TODO: Delete! This is legacy from old game design when worker ants were able to create StockFight by themselves
    private bool _isFightStockVisible = true;

    public FightStockPresenter(int placeNum) : base(FoodType.FightStock, placeNum)
    {
        if (_isFightStockVisible)
            GameEvents.OnFightStockBecameVisible?.Invoke(this);
    }
    public override void Restock() => Add(Constants.RestockFightStockAmount);
    public override float Add(float quantity)
    {
        if (IsFightStock && !_isFightStockVisible)
        {
            _isFightStockVisible = true;

            GameEvents.OnFightStockBecameVisible?.Invoke(this);
        }

        return base.Add(quantity);
    }
}