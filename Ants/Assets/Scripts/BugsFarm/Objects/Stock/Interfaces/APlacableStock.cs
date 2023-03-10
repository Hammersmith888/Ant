using System;

[Serializable]
public abstract class APlacableStock<T> : APlaceable_T<T>, IStock where T: A_MB_Placeable 
{
    public APlacableStock(ObjType type, int placeNum, int subType = 0) : base(type, subType, placeNum) { }
    public abstract float QuantityCur { get; protected set; }
    public abstract float QuantityMax { get; protected set; }
    public abstract void Restock();

    public abstract float Add(float quantity);
    public virtual float Remove(float quantity) => 0;
    public abstract void SetQuantity(float quantity);
    public abstract void Reset();
}