using System;
using System.Collections.Generic;

namespace BugsFarm.Model
{
    [Serializable]
    public class RoomData
    {
        public List<CfgEnemy> enemies;
        public int reward_Coins;
        public int reward_Crystals;
    }
}