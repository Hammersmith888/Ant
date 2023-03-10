using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.Game;
using BugsFarm.UnitSystem.Obsolete;
using UnityEngine;

public static class Keeper
{
    public static List<Maggot> Maggots { get; private set; } = new List<Maggot>();
    public static AutoKeys AntIDs { get; private set; } = new AutoKeys();
    public static List<AntPresenter> Ants { get; private set; } = new List<AntPresenter>();

    public static Dictionary<ObjType, List<APlaceable>> Objects { get; private set; } = new Dictionary<ObjType, List<APlaceable>>();

    public static readonly List<APlaceable> Buildings = new List<APlaceable>();
    public static Dictionary<QuestID, Quest> Quests { get; private set; } = new Dictionary<QuestID, Quest>();

    public static Stats Stats { get; private set; } = new Stats();

    private static readonly List<APlaceable> _empty = new List<APlaceable>();
    
    
    public static void Clear()
    {
        Clear(Ants);
        Clear(Maggots);

        foreach (var objs in Objects)
        {
            Clear(objs.Value);
        }

        Quests.Clear();

        Stats.Reset();
    }

    private static MonoFactory _monoFactory;
    //private static DayNight _dayNight;
    //public static void Init(DayNight dayNight, MonoFactory monoFactory)
    //{
    //    _monoFactory = monoFactory;
    //    _dayNight = dayNight;
    //    Stats.gameStartUTC = DateTime.UtcNow;
//
    //    Create_PreCreated();
    //    //menu_Quests.Instance.InitQuests();
    //    dayNight.PostSpawnInit();
    //}

    private static void Create_PreCreated()
    {
        // _monoFactory.Spawn(Constants.QueenPlace, ObjType.Queen);
        // _monoFactory.Spawn(Constants.BowlPlace, ObjType.Bowl);
        // _monoFactory.Spawn(Constants.QueenLightPlace, ObjType.Decoration, (int)DecorType.Light);
        // _monoFactory.Spawn(Constants.DigGroundPlace, ObjType.DigGroundStock);
        //Factory.Instance.Spawn(Constants.SafePlace, ObjType.str_Safe);
        //Factory.Instance.Spawn(Constants.RelaxationRoomPlace, ObjType.str_RelaxationRoom);
    }
    private static void Clear<T>(ICollection<T> items) where T : IDisposable
    {
        foreach (var obj in items)
        {
            obj.Dispose();
        }

        items.Clear();
    }
    private static void Book<T>(T obj, ICollection<T> objs)
    {
        objs.Add(obj);
    }
    private static void Destroy<T>(T obj, ICollection<T> objs) where T : IDisposable
    {
        objs.Remove(obj);
        obj.Dispose();
    }
    
    public static void Book(Maggot maggot) { Book(maggot, Maggots); }
    public static void Destroy(Maggot maggot) { Destroy(maggot, Maggots); }

    public static void Book(AntPresenter ant) { Book(ant, Ants); }
    public static void Destroy(AntPresenter ant) { Destroy(ant, Ants); GameEvents.OnAntDestroyed?.Invoke(ant); }
    
    public static void Book(APlaceable obj)
    {
        if (!Objects.ContainsKey(obj.Type))
        {
            Objects.Add(obj.Type, new List<APlaceable>());
        }
        
        Book(obj, Objects[obj.Type]);
    }
    public static void Destroy(APlaceable obj)
    {
        Destroy(obj, Objects[obj.Type]);
    }
    
    public static List<APlaceable> GetObjects(ObjType type)
    {
        return Objects.TryGetValue(type, out var value) ? value : _empty;
    }
    public static IEnumerable<T> GetObjects<T>()
    {
        return Objects.SelectMany(x => x.Value).OfType<T>();
    }

    public static void Pack(GameData gameData)
    {
        gameData.maggots = Maggots;

        gameData.ant_IDs = AntIDs;
        gameData.ants = Ants;

        gameData.objects = Objects;
        gameData.quests = Quests;
        gameData.stats = Stats;
    }
    public static void Unpack(GameData gameData)
    {
        Clear();
        
        Unpack_Objects(gameData);
        Unpack_Ants(gameData);
        
        Quests = gameData.quests;
        Stats = gameData.stats;

        //_dayNight.PostLoadRestore();
    }
    private static void Unpack_Ants(GameData gameData)
    {
        
        Maggots = gameData.maggots;

        AntIDs = gameData.ant_IDs;
        Ants = gameData.ants;
        
        foreach (var maggot in gameData.maggots)
        {
            _monoFactory.Create_MB(maggot);
            maggot.PostLoadRestore();
        }

        foreach (var ant in gameData.ants)
        {
            _monoFactory.Create_MB(ant);
            ant.PostLoadRestore();
        }
    }
    private static void Unpack_Objects(GameData gameData)
    {
        Objects = gameData.objects;

        foreach (var objectsOfOneType in gameData.objects)
        {
            foreach (var placeable in objectsOfOneType.Value)
            {
                _monoFactory.Create_MB(placeable);
                placeable.PostLoadRestore();
            }
        }
    }
}

