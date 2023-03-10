

public abstract class A_MB_Placeable_T<T> : A_MB_Placeable where T : APlaceable
{
    protected T _placeable;
    protected override APlaceable placeable => _placeable;
    public override void Init(APlaceable placeable)
    {
        _placeable = (T)placeable;
    }
}

