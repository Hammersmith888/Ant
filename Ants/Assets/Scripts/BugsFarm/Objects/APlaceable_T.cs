using System;


[Serializable]
public abstract class APlaceable_T<T> : APlaceable where T : A_MB_Placeable
{
    public T Mb => _mb;

    [NonSerialized] protected T _mb;
    protected override A_MB_Placeable MbPlaceable => _mb;

    public override void Init(A_MB_Placeable mb)
    {
        _mb = (T)mb;
    }
    protected APlaceable_T(ObjType type, int subType, int placeNum) : base(type, subType, placeNum)  { } 
}

