using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public struct CfgEnemy
{
    public AntType type;
    public int level;
}

[Serializable]
public class CfgRoomEnemies
{
    public List<CfgEnemy> enemies;
    public int reward_Coins;
    public int reward_Crystals;
}

[CreateAssetMenu(fileName = ScrObjs.CfgEnemies, menuName = ScrObjs.folder + ScrObjs.CfgEnemies,
    order = ScrObjs.CfgEnemies_i)]
public class CfgEnemies : ScriptableObject
{
    public Data_Fight.RoomsConfig roomsConfig;
}