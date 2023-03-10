using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.BuildingSystem;
using UnityEngine;

[Serializable]
public class WorkPlace : APublisher, IPostLoadRestorable
{
    [field: NonSerialized] public event Action OnComplete;
    [field: NonSerialized] public event Action<int> OnProcess;
    public bool IsCurrentJob { get; protected set; }
    public virtual bool IsCompleted { get; private set; } = false;
    public virtual int QuantityCur => _quantityCur;
    public virtual int QuantityMax => _quantityMaxMem;
    public virtual int OcuppyCount => _ocuppyCount;
    [field:NonSerialized] public bool IsWorkDown { get; private set; }

    [NonSerialized] private Dictionary<IPosSide, int> _occupiedPoses;
    [NonSerialized] private IPosSide[] _points;
    [NonSerialized] private int _quantityMaxMem = 0;
    [NonSerialized] private int _maximumWorkers = 0;
    private int _ocuppyCount = 0;
    private int _quantityCur = int.MaxValue;
    private readonly ObjEvent _objEvent;
    public WorkPlace(ObjEvent objEvent)
    {
        _objEvent = objEvent;
    }

    public void Reset()
    {
        IsCurrentJob = false;
        _quantityCur = -1;
        IsCompleted = false;
        _ocuppyCount = 0;
        InitDefault();
    }
    public virtual void EndJob()
    {
        IsCurrentJob = false;
        CastObjectEvent(_objEvent);
        _quantityCur = 0;
        IsCompleted = true;
    }
    public virtual void Init(int maximumWorkers, IPosSide[] points, int quantityMax, bool isWorkDown)
    {
        _maximumWorkers = maximumWorkers;
        IsWorkDown = isWorkDown;
        _occupiedPoses = new Dictionary<IPosSide, int>();
        _quantityMaxMem = quantityMax;
        _points = points;
        InitDefault();
    }
    public virtual void PostLoadRestore()
    {
        if (_points.IsNullOrDefault() || _points.Length == 0)
        {
            Debug.LogError($"{this} : Мест для оккупаций нет, инициализация после загрузки не выполенна!!!");
            return;
        }
        var occupersToFill = _ocuppyCount;
        while (occupersToFill > 0) // заполнить максимально равномерно оккупантами
        {
            foreach (var pose in _points)
            {
                if (occupersToFill == 0) break;

                if(!_occupiedPoses.ContainsKey(pose))
                {
                    _occupiedPoses.Add(pose, 0);
                }
                _occupiedPoses[pose] += 1;
                occupersToFill--;
            }
        }
    }
    public virtual bool Process(int quantity)
    {
        if (!IsCurrentJob)
        {
            Debug.LogException(new Exception($"{this} : Работа идет но, работа не является текущей, эту работу делать нельзя!"));
            return false;
        }

        OnProcess?.Invoke(_quantityCur = Mathf.Max(_quantityCur - quantity, 0));
        
        if (_quantityCur > 0)
        {
            return false;
        }
        
        EndJob();
        OnComplete?.Invoke();
        return true;
    }
    public virtual void OnWorkSelected()
    {
        if (!IsCurrentJob)
        {
            IsCurrentJob = true;
            _quantityCur = _quantityMaxMem;
        }

        OnProcess?.Invoke(_quantityCur = Mathf.Max(_quantityCur, 0));
    }
    
    public virtual void FreeOccupy(IPosSide pointToFree)
    {
        if (_ocuppyCount > 0)
        {
            FreePosition(pointToFree);
        }
        else
        {
            Debug.LogError("Не занято ни одного места!!!");
        }
    }
    public virtual bool TryOccupy()
    {
        return _ocuppyCount < _maximumWorkers;
    }
    public virtual IPosSide Occupy()
    {
        if(!TryOccupy())
        {
            Debug.LogError("Невозомжно окупировать, заняты все места!!!");
            return null;
        }
        
        var posSide = _occupiedPoses.OrderBy(x => x.Value).First().Key; // получаем место где меньше всего занято
        _occupiedPoses[posSide] += 1; // занимаем
        _ocuppyCount++;
        return posSide;
    }
    public override void Dispose()
    {
        base.Dispose();
        _points = null;
        _occupiedPoses = null;
        IsCurrentJob = false;
        IsCompleted = false;
        _ocuppyCount = 0;
        _quantityCur = _quantityMaxMem;
    }

    protected virtual void FreePosition(IPosSide pointToFree)
    {
        var point = _occupiedPoses.Keys.FirstOrDefault(x => Equals(x, pointToFree));
        if(point.IsNullOrDefault())
        {
            return;
        }
        _ocuppyCount--;

        var countWorkers = _occupiedPoses[point];
        if(countWorkers > 0)
        {
            _occupiedPoses[point] -= 1;
        }
    }
    protected virtual void InitDefault()
    { 
        _occupiedPoses.Clear();
        foreach (var pose in _points)
        {
            if (!_occupiedPoses.ContainsKey(pose))
            {
                _occupiedPoses.Add(pose, 0);
            }
        }
    }
}
[Serializable]
public class BuildVinePlace : WorkPlace
{
    public BuildVinePlace(ObjEvent objEvent) : base(objEvent){}
}
[Serializable]
public class DigPlace : WorkPlace
{
    public DigPlace(ObjEvent objEvent) : base(objEvent){}
}
