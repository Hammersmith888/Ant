using System;
using BugsFarm.Installers;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;

public class Data_Fight : MB_Singleton<Data_Fight>
{
    [Serializable]
    public class TUnits : SerializableDictionaryBase<AntType, CfgUnit>
    {
    }

    [Serializable]
    public class RoomsConfig : SerializableDictionaryBase<int, CfgRoomEnemies>
    {
    }

    [Serializable]
    public class UnitViewConfig : SerializableDictionaryBase<AntType, UnitViewSettingsInstaller.UnitViewItem>
    {
    }
    
    [Serializable]
    public class UnitFightConfig : SerializableDictionaryBase<AntType, UnitFightSettingsInstaller.UnitFightItem>
    {
    }

    public TUnits units;
    public CfgEnemies enemies;

    public CfgUnit GetUnitConfig(AntType antType)
    {
        if (!units.ContainsKey(antType))
        {
            Debug.LogError($"{this} : Коллекция конфигов юнита не содержит ключ : {antType}");
            return default;
        }

        return units[antType];
    }

    [ExposeMethodInEditor]
    private void Migrate()
    {
        /*var rooms = enemies.rooms;
        var roomsConfig = enemies.roomsConfig;
        var index = 0;

        roomsConfig.Clear();

        foreach (var room in rooms)
        {
            if (index > 0 && index % 20 == 0)
            {
                roomsConfig.Add(index, null);
                index++;
            }

            roomsConfig.Add(index, room);
            index++;
        }*/
    }
}