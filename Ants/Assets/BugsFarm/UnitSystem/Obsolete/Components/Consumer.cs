using System;

namespace BugsFarm.UnitSystem.Obsolete.Components
{
    [Serializable]
    public class Consumer : IPostSpawnInitable
    {
        public bool IsHungry { get; private set; }
        public Timer TimerNeed { get; private set; } = new Timer();
        public Timer TimerHunger { get; private set; } = new Timer();

        private float _needTime;
        private float _daysWithoutConsume;
        private bool _hungryAfterSpawn;

        /// <summary>
        /// needTime - время в минутах
        /// </summary>
        /// <param name="needTime">время в минутах</param>
        /// <param name="daysWithoutConsume"></param>
        /// <param name="hungryAfterSpawn"></param>
        public void Init(float needTime, float daysWithoutConsume, bool hungryAfterSpawn)
        {
            _needTime = needTime * 60;
            _daysWithoutConsume = daysWithoutConsume;
            _hungryAfterSpawn = hungryAfterSpawn;
        }
        public void PostSpawnInit()
        {
            var randomTimeNeed = _hungryAfterSpawn ? 0 : UnityEngine.Random.Range(0, _needTime);
            TimerNeed.Set(randomTimeNeed);
        }
        public void Update()
        {
            if (IsHungry == false && TimerNeed.IsReady)
            {
                IsHungry = true;
                var hungerTime = DaysToSeconds(_daysWithoutConsume);
                TimerHunger.Set(hungerTime);
            }
        }
        public void ConsumptionStart()
        {
            IsHungry = false;
        }
        public void ConsumptionEnd(float consumed, float consumptionMax)
        {
            IsHungry = false;
            TimerNeed.Set(_needTime * (consumed / consumptionMax));
        }
        private float DaysToSeconds(float days)
        {
            const int hoursInDay = 24;
            const int minsInHour = 60;
            const int secondsInHour = 60;
            return days * hoursInDay * minsInHour * secondsInHour;
        }
    }
}

