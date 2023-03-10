using System;

namespace BugsFarm.AnimationsSystem
{
    [Serializable]
    public enum AnimKey
    {
        None			= 0,

        Walk			= 2,
        WalkClimb		= 1,
        WalkWater		= 45,
        WalkHerbs		= 46,
        WalkMud		    = 11,
        WalkFood		= 16,
        WalkGarbage		= 32,
        
        Run				= 41,
        Fly	            = 44,

        Put				= 25,
        TakeFoodLow	    = 28,
        TakeFoodMid	    = 15,
        TakeFoodHi		= 29,
        TakeGarbage		= 31,
        
        GiveFood		= 17,
        GiveHerbs		= 47,
        GiveWater		= 48,
        Birth    		= 49,
        
        Idle			= 3,
        Idle1			= 50,
        Idle2			= 51,
        Drink			= 4,
        Eat				= 5,
        Sleep			= 30,
        Awake			= 40,
        Death			= 14,

        MineLever		= 8,
        MineRock		= 9,

        DigMidle	= 10,
        DropMud		= 12,
        DigLow		= 13,
        
        JumpOff			= 18,
        JumpFall		= 19,
        JumpLand		= 20,

        Attack			= 26,
        Attack2			= 27,
        FightHit		= 21,
        FightKick		= 22,
        FightIdle		= 23,
        FightShovel1	= 42,
        FightShovel2	= 43,

        Training		= 24,

        GarbageDropPile				= 33,
        GarbageDropRecycler			= 34,

        Garden1						= 35,
        Garden2						= 36,

        Build1						= 37,
        Build2						= 38,
        Build3						= 39,
        Build4						= 52,
        Build5						= 53,
    }
}