using System.Collections.Generic;

public class GPath
{
    public int Step { get; private set; }
    public GStep CurStep => _path[Step];
    public GStep PrevStep => _path[Step - 1];
    public GStep NextStep => _path[Step + 1];

    private List<GStep> _path = new List<GStep>();

    public GStep this[int index]
    {
        get => _path[index];
        set => _path[index] = value;
    }
    public bool Find(GPos source, GPos target)
    {
        bool success = Graph.Instance.FindPath(_path, source, target);

        if (success)
            Step = 0;

        return success;
    }
    public bool Proceed()
    {
        return ++Step == _path.Count;
    }
    public bool IsLastStep()
    {
        return Step >= _path.Count - 1;
    }
}

