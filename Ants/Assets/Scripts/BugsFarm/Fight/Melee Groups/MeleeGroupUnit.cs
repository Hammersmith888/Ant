using System;

[Serializable]
public class MeleeGroupUnit
{
    public Unit unit;

    public MeleeGroup group;
    public float? pos;


    public MeleeGroupUnit(Unit unit)
    {
        this.unit = unit;
    }
}