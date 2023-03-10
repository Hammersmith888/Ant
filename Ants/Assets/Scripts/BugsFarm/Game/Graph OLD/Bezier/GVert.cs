using UnityEngine;


public enum GVertType
{
    Road,
    Ladder,
    Joint
}


public class GVert : MonoBehaviour
{
    public GVertType Type => _type;
    public bool IsLadder => _type == GVertType.Ladder;

    public float Length => _polyline.Length;
    public bool RightDir { get; private set; }
    public Vector2 P_xMin { get; private set; }
    public Vector2 P_xMax { get; private set; }
    public float Y_min => _polyline.Y_min;
    public float Y_max => _polyline.Y_max;

    public GVert Joint_t0 { get; private set; }
    public GVert Joint_t1 { get; private set; }

    public float WalkTarget_tMin { get; private set; }
    public float WalkTarget_tMax { get; private set; }
    public int PlacesGroup => _placesGroup;


#pragma warning disable 0649

    [SerializeField] BezierCurve _bezier;
    [SerializeField] GVertType _type;
    [SerializeField] int _placesGroup;

#pragma warning restore 0649

    private readonly Polyline _polyline = new Polyline();

    public void Init()
    {
        if (_type == GVertType.Joint)
            return;


        _polyline.Init(_bezier);


        Vector2 p0 = _polyline.GetPointAt(0);
        Vector2 p1 = _polyline.GetPointAt(1);

        RightDir = p0.x < p1.x;
        P_xMin = RightDir ? p0 : p1;
        P_xMax = RightDir ? p1 : p0;
    }
    public void Stretch()
    {
        _polyline.Stretch(Joint_t0?.transform.position, Joint_t1?.transform.position);
    }
    public void SetJoint_t0(GVert joint) { Joint_t0 = joint; }
    public void SetJoint_t1(GVert joint) { Joint_t1 = joint; }
    public void SetWalkTarget_tMin(float t) { WalkTarget_tMin = t; }
    public void SetWalkTarget_tMax(float t) { WalkTarget_tMax = t; }
    public Vector2 GetPointAt(float t)
    {
        return _polyline.GetPointAt(t);
    }
    public Vector2 GetPointNormalAt(float t)
    {
        return _polyline.GetPointNormalAt(t);
    }
}

