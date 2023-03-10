using UnityEngine;

namespace BugsFarm.SpeakerSystem
{
    public struct SpeakerParams
    {
        public Vector2Int CountSpeakersRange;
        public Vector2 DelayRange;
        public float LifeTime;
        public bool IsGreeting;

    
        public static SpeakerParams Normal = new SpeakerParams
        {
            IsGreeting = false,
            CountSpeakersRange = new Vector2Int(3, 6 + 1),
            //DelayRange = new Vector2(9, 12),
            DelayRange = new Vector2(4.5f, 6),
            LifeTime = 5,
        };
    
        public static SpeakerParams Greeting = new SpeakerParams
        {
            IsGreeting = true,
            CountSpeakersRange = new Vector2Int(2, 3 + 1),
            DelayRange = new Vector2(4, 4),
            LifeTime = 3,
        };
    }
}