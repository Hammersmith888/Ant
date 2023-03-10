using System;
using BugsFarm.BuildingSystem;
using UnityEngine;

[Serializable]
public class HerbsStockPresenter : APlacableStock<MB_Stock>
{
    protected override IPosSide[] PointsBuilding => _mb.PointsBuidling;
    public bool IsFull => QuantityCur >= QuantityMax;
    public bool IsDepleted => QuantityCur == 0;
    public override float QuantityMax { get; protected set; } = 100;
    public override float QuantityCur { get; protected set; }
   
    public HerbsStockPresenter(int placeNum) : base(ObjType.HerbsStock, placeNum)
    {
        QuantityCur = 0;
    }
    public override void PostSpawnInit()
    {
        base.PostSpawnInit();
        Mb.OnPlaceSelected_Yes(PlaceNum);
    }
    public override void Restock()
    {
        QuantityCur = QuantityMax;
    }
    public override float Add(float quantity)
    {
        var old = QuantityCur;
        QuantityCur = Mathf.Min(QuantityCur + quantity, QuantityMax);
        if (IsFull)
        {
            CastObjectEvent(ObjEvent.IsFull);
        }
        return old - QuantityCur; // delta
    }
    public override float Remove(float quantity)
    {
        var old = QuantityCur;
        QuantityCur = Mathf.Min(QuantityCur - quantity, 0);
        if(IsDepleted)
        {
            CastObjectEvent(ObjEvent.IsDepleted);
        }

        return old - QuantityCur; // delta
    }
    public override void SetQuantity(float quantity)
    {
        QuantityCur = quantity;
    }
    public override void Reset()
    {
        QuantityCur = 0;
    }
}