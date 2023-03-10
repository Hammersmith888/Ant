namespace BugsFarm.SimulatingSystem
{
    public class Timer
    {
        public bool IsExpired => _timeLeft <= 0;

        private float _timeLeft;

        public Timer(float initialTime)
        {
            _timeLeft = initialTime;
        }

        public void SubtractTime(float value)
        {
            _timeLeft -= value;
        }
    }
}