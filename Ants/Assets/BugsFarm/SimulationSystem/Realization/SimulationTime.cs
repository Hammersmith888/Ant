using UnityEngine;

namespace BugsFarm.SimulationSystem
{
    /// <summary>
    /// Позволяет контролировать тик симуляции и хранит дельту времени обновления.
    /// Постепенно употребляя время можно регулировать
    /// может ли задача/работа выполенна в этот тик симуляции или нет.
    /// </summary>
    public class SimulationTime
    {
        private float _tickTime;
        
        public bool Available()
        {
            return _tickTime > 0;
        }
        
        public void ResetWith(float seconds)
        {
            _tickTime = seconds;
        }
        public bool Apply(float seconds)
        {
            if (_tickTime >= seconds)
            {
                _tickTime = Mathf.Max(0, _tickTime - seconds);
                return true;
            }

            _tickTime = 0;
            return false;
        }
    }
}