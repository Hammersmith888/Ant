using System;

[Serializable]
public class PileStockPresenter : FoodStockPresenter
{
    public override bool IsGarbage => true;
    public PileStockPresenter(int placeNum) : base(FoodType.PileStock, placeNum){}
    protected override void OnDepleted()
    {
        base.OnDepleted();
        Keeper.Destroy(this);
    }
}