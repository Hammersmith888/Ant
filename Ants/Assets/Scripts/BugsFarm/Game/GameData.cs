using System;
using System.Collections.Generic;
using BugsFarm.UnitSystem.Obsolete;

[Serializable]
public class GameData
{
    public GameData()
    {
        gameAge = 0;
        exitTime = 0;

        coins = crystals = 0;
        quests = new Dictionary<QuestID, Quest>();
        maggots = new List<Maggot>();
        ants = new List<AntPresenter>();
        ant_IDs = new AutoKeys();
        objects = new Dictionary<ObjType, List<APlaceable>>();
        defaultSquad = new List<AntPresenter>();
        fightRoomIndex = 0;
        stats = new Stats();
    }

    public bool Replace(GameData another)
    {
        if (another.IsNullOrDefault())
        {
            FirstPlay = true;
            return false;
        }
        gameAge = another.gameAge;
        exitTime = another.exitTime; 
    
        coins = another.coins;
        crystals = another.crystals;
    
        //Rooms = another.Rooms;
        quests = another.quests;
        
        maggots = another.maggots;
        ants = another.ants;
        ant_IDs = another.ant_IDs;
        
        objects = another.objects;
    
    
        defaultSquad = another.defaultSquad;
        fightRoomIndex = another.fightRoomIndex;
    
        stats = another.stats;
        FirstPlay = false;
        return true;
    }

    public void Reset()
    {
        //Replace(new GameData());
        FirstPlay = true;
    }

    public bool FirstPlay { get; private set; } = true;
    public double gameAge;
    public double exitTime;         // UTC

    public int coins;
    public int crystals;

    //public RoomsContoller Rooms;
    public Dictionary<QuestID, Quest> quests;
    
    public List<Maggot> maggots;
    public List<AntPresenter> ants;
    public AutoKeys ant_IDs;
    
    public Dictionary<ObjType, List<APlaceable>> objects;


    public List<AntPresenter> defaultSquad;
    public int fightRoomIndex;

    public Stats stats;
}

