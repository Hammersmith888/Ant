using System;


[Serializable]
public struct SGSide
{
    public SGPos gPos;
    public bool lookLeft;


    public SGSide(SGPos gPos, bool lookLeft)
    {
        this.gPos = gPos;
        this.lookLeft = lookLeft;
    }


    //public static implicit operator SGSide(PosSide p) => new SGSide(p.GPos, p.LookLeft);
}

