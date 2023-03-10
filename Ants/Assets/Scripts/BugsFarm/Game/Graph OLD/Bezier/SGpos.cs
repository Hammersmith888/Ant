using System;


[Serializable]
public struct SGPos
{
    public int gVert_key;
    public float t;


    public SGPos(int gVert_key, float t)
    {
        this.gVert_key = gVert_key;
        this.t = t;
    }


    public static implicit operator SGPos(GPos p) => new SGPos(Graph.Instance.GVert_2_Key(p.gVert), p.t);
    public static implicit operator GPos(SGPos sp) => new GPos(Graph.Instance.Key_2_GVert(sp.gVert_key), sp.t);
}

