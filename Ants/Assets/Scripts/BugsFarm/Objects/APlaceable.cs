using System;
using System.Collections.Generic;
using BugsFarm.BuildingSystem.Obsolete;
using BugsFarm.Config;
using BugsFarm.Game;
using Object = UnityEngine.Object;

[Serializable]
public abstract class APlaceable : ABuilding, IPostSpawnInitable, IPostLoadRestorable, IDisposable
{
    public virtual string Header => Data_Objects.Instance.GetData(Type,SubType).wiki.Header;
    public float TimeLeft => BuildUpgradeTimer.Left;
    public bool IsUpgradable => UpgradeLevels != null && Level < UpgradeLevels.Count - 1;
    public UpgradeLevel UpgradeLevelCur => UpgradeLevels[Level];
    public UpgradeLevel? UpgradeLevelNxt => IsUpgradable ? UpgradeLevels[Level + 1] : (UpgradeLevel?)null ;
    public Timer BuildUpgradeTimer { get; protected set; }= new Timer();
    
    public ObjType Type { get; private set; }
    public int SubType { get; private set; }
    public int PlaceNum { get; private set; }
    public int Level { get; private set; }
    public bool IsReady { get; private set; }

    [NonSerialized] protected List<UpgradeLevel> UpgradeLevels;

    protected abstract A_MB_Placeable MbPlaceable { get; }

    public abstract void Init(A_MB_Placeable mbPlaceable);
    public virtual void PostSpawnInit()
    {
        var isReady = UpgradeLevels == null || UpgradeLevels[0].minutes <= 0 ;

        // (!) BEFORE SetPlace()
        if (isReady)
        {
            SetIsReady(true);
        }
        else
        {
            SetBuildTimer();
        }

        SetPlace();

        // // (!) AFTER SetPlace()
        // FindWorkerIfRequired();
    }
    public virtual void PostLoadRestore()
    {
        if (!IsReady)
        {
            Keeper.Buildings.Add(this);
        }
        SetUpgradeLevels();

        SetPlace();
    }
    public void SetPlace(int? placeNum = null)
    {
        if (placeNum.HasValue)
            PlaceNum = placeNum.Value;

        OccupiedPlaces.Occupy(this);

        MbPlaceable.SetPlacePos();
    }
    public virtual void OnReplaced()
    {
        CastObjectEvent(ObjEvent.Moved);
    }
    public override void Dispose()
    {
        base.Dispose();
        
        Keeper.Buildings.Remove(this);
        
        GameEvents.OnObjectDestroyed?.Invoke(this);
        
        OccupiedPlaces.Free(this);

        Object.Destroy(MbPlaceable.gameObject);

        Panel_SelectPlace.Instance.OnDestroyed(MbPlaceable);
    }
    public virtual void Update()
    {
        SetIsReady();
    }
    public void SpeedUp()
    {
        BuildUpgradeTimer.Unpause();
        BuildUpgradeTimer.Set(0);

        SetIsReady(true);
    }
    public virtual void Upgrade()
    {
        Level++;

        SetBuildTimer();
    }
    public void SetColliderAllowed(bool allowed)
    {
        // MbPlaceable.SetColliderAllowed(allowed);
    }

    protected APlaceable(ObjType type, int subType, int placeNum)
    {
        Type = type;
        SubType = subType;
        PlaceNum = placeNum;

        SetUpgradeLevels();
    }
    private void SetUpgradeLevels()
    {
        // Data_Objects.Instance can be NULL, because this method is called from default Ctor(), which is called at the very start for Serializable classes

        UpgradeLevels = Data_Objects.Instance?.GetData(Type, SubType)?.upgrades?.levels;
    }
    private void SetIsReady()
    {
        SetIsReady(BuildUpgradeTimer.IsReady);
    }
    private void SetIsReady(bool value)
    {
        var prev = IsReady;
        IsReady = value;

        // (!) AFTER IsReady set
        // Если постройка завершина - то будет единичный вызов кек
        if (!prev && value)
        {
            CastObjectEvent(ObjEvent.BuildUpgradeEnd);
            Keeper.Buildings.Remove(this);
            if (Level == 0)
            {
                GameEvents.OnObjectLevel1BuildComplete?.Invoke(this);
            }
            else
            {
                GameEvents.OnObjectUpgrade?.Invoke(this);
            }
        }
    }
    private void SetBuildTimer()
    {
        if (UpgradeLevelCur.minutes <= 0)
            return;

        if (!Keeper.Buildings.Contains(this))
        {
            Keeper.Buildings.Add(this);
        }

        CastObjectEvent(ObjEvent.BuildUpgradeBgn);
        BuildUpgradeTimer.Set(UpgradeLevelCur.minutes * 60);
        BuildUpgradeTimer.Pause();
        GameEvents.OnObjectUpgradeBgn?.Invoke(this);
        SetIsReady();
    }

    public override void FreeBuilding()
    {
        base.FreeBuilding();
        if (!BuildUpgradeTimer.IsReady)
        {
            BuildUpgradeTimer.Pause();
        }
    }
}

