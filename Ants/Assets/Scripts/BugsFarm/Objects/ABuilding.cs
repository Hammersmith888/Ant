using System;
using BugsFarm.BuildingSystem;
using Random = UnityEngine.Random;


[Serializable]
public abstract class ABuilding : APublisher
{
    private const int _maxOccupants = 1;
    private int _occupantsCount = 0;

    protected virtual IPosSide[] PointsBuilding => null;

    public bool TryOccupyBuilding()
    {
        return _occupantsCount < _maxOccupants;
    }
    public bool OccupyBuilding()
    {
        if (!TryOccupyBuilding())
        {
            return false;
        }

        _occupantsCount++;

        return true;
    }
    public virtual void FreeBuilding()
    {
        _occupantsCount--;
    }
    public IPosSide GetRandomPosition(IPosSide exclude = null)
    {
        var i = Random.Range(0, PointsBuilding.Length);
        var point = PointsBuilding[i];

        if (exclude != null)
        {
            i = (i + (Equals(point, exclude) ? 1 : 0)) % PointsBuilding.Length;
            point = PointsBuilding[i];
        }

        return point;
    }
}

