using System;
using System.Collections.Generic;
using BugsFarm.BuildingSystem;
using UnityEngine;
using Random = UnityEngine.Random;


[Serializable]
public class Occupyable<T> : IPostSpawnInitable
{
    public List<int> Free { get; } = new List<int>();
    public Dictionary<T, int> Occupied { get; } = new Dictionary<T, int>();

    [NonSerialized] private IPosSide[] _points;

    public void Init(IPosSide[] points)
    {
        _points = points;
    }
    public void PostSpawnInit()
    {
        for (var i = 0; i < _points.Length; i++)
        {
            Free.Add(i);
        }
    }
    public bool OccupyRandom(T ant, out IPosSide point)
    {
        if (!TryOccupy())
        {
            Debug.LogError($"{this} : OccupyRandomPos - не получилось случайно оккупировать позицию!!!");
            point = default;
            return false;
        }

        point = GetRandomFreePos(out var index) ? Occupy(ant, index) : null;
        return true;
    }
    public bool TryOccupy()
    {
        return Free.Count > 0;
    }
    public IPosSide Occupy(T occupant, int index)
    {
        if (!TryOccupy())
        {
            Debug.LogError($"{this} : OccupyPos - не получилось оккупировать позицию!!!");
            return default;
        }            
        Occupied.Add(occupant, index);
        Free.Remove(index);
        return _points[index];
    }
    public bool FreeOccupy(T occupant)
    {
        if(!Occupied.ContainsKey(occupant))
        {
            Debug.LogError($"{this}: Оккупант не существует в коллекции окупации!!!");
            return false;
        }
        var index = Occupied[occupant];

        Occupied.Remove(occupant);
        Free.Add(index);

        return true;
    }
    public void FreeAll()
    {
        if(Occupied.Values.Count > 0)
        {
            Free.AddRange(Occupied.Values);
        }
        Occupied.Clear();
    }
    private bool GetRandomFreePos(out int index)
    {
        if (!TryOccupy())
        {
            Debug.LogError($"{this} : GetRandomFreePos - не получилось оккупировать позицию!!!");
            index = -1;
            return false;
        } 

        index = Free[Random.Range(0, Free.Count)];

        return true;
    }
}

