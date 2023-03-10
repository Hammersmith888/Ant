using System;
using BugsFarm.BuildingSystem;
using BugsFarm.UnitSystem;
using BugsFarm.UnitSystem.Obsolete;
using UnityEngine;

[Serializable]
public abstract class AConsumable : APlaceable
{
    public float QuantityCur { get; protected set; }
    public float QuantityMax { get; protected set; }
    public bool IsFull => QuantityCur >= QuantityMax;
    public bool IsDepleted => QuantityCur <= 0;
    protected override IPosSide[] PointsBuilding => MbConsumable.Points;
    protected override A_MB_Placeable MbPlaceable => MbConsumable;
    protected abstract A_MB_Consumable MbConsumable { get; }

    private Occupyable<ATask> _occupyable = new Occupyable<ATask>();

    protected AConsumable(ObjType type, int subType, int placeNum, float quantityMax, bool full = true)
        : base(type, subType, placeNum)
    {
        QuantityCur = full ? quantityMax : 0;
        QuantityMax = quantityMax;
    }
    public override void PostSpawnInit()
    {
        base.PostSpawnInit();
        _occupyable.PostSpawnInit();
    }
    public virtual float Add(float quantity)
    {
        var before = QuantityCur;

        SetQuantity(Mathf.Clamp(QuantityCur + quantity, 0, QuantityMax));

        return QuantityCur - before; // actual delta
    }
    public bool Consume(float consumeMax, float singlePassConsume, ref float consumed)
    {
        // сколько еще нужно
        var need = consumeMax - consumed; 
        
        // требуемое потребление
        var required = Mathf.Min(singlePassConsume, need);

        bool done;
        var depleted = false;

        // Если доступно меньше, или равно требуемого
        if (QuantityCur <= required)
        {
            // Water out
            depleted = true;
            done = true;
            consumed += QuantityCur;
            QuantityCur = 0;
        }
        // Если к употреблению нужно меньше или равно базовому употреблению за раз
        else if (need <= singlePassConsume)
        {
            // Drinking complete
            done = true;
            consumed = consumeMax;
            QuantityCur -= need;
        }
        else
        {
            // Drinking in process
            done = false;
            consumed += required;
            QuantityCur -= required;
        }


        // Don't leave 0.000001 food
        if (QuantityCur < .01f)
        {
            QuantityCur = 0;
            depleted = true;
            done = true;
        }


        SetSprite();
        
        if (depleted)
            OnDepleted();

        return done;
    }
    
    public IPosSide GetPointSide(ATask occupant)
    {
        return PointsBuilding[_occupyable.Occupied[occupant]];
    }
    public bool OccupyConsumable(ATask occupant, out IPosSide point)
    {
        return _occupyable.OccupyRandom(occupant, out point);
    }
    public bool TryOccupyConsumable(bool zeroQuantityAllowed = false)
    {
        return (zeroQuantityAllowed || !IsDepleted) && _occupyable.TryOccupy();
    }
    public void FreeOccupy(ATask occupant)
    {
        _occupyable.FreeOccupy(occupant);
    }
    protected void InitOccupyable()
    {
        _occupyable.Init(PointsBuilding);
    }
    
    protected virtual void OnDepleted()
    {
        CastObjectEvent(ObjEvent.IsDepleted);
    }
    protected void SetToMax()
    {
        SetQuantity(QuantityMax);
    }
    protected void SetQuantity(float quantity)
    {
        QuantityCur = quantity;

        SetSprite();

        // Depleted
        if (QuantityCur <= 0)
            OnDepleted();
    }
    protected abstract void SetSprite();
}

