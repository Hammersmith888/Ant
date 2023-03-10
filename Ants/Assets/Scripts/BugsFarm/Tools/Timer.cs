using System;
using BugsFarm.SimulationSystem;
using BugsFarm.SimulationSystem.Obsolete;
using UnityEngine;

[Serializable]
public enum TimerType
{
    Simulation,
    Scaled,
    Unscaled,
}

[Serializable]
public class Timer
{
    public float Duration => _duration;
    public double End => _timeEnd;
    public bool IsReady => Time >= _timeEnd;
    public bool IsPaused => _isPaused;
    public float Progress => _duration <= 0 ? 1 : (float)Tools.Clamp01((Time - _timeBegin) / _duration);
    public float Left => Mathf.Max((float)(_timeEnd - Time), 0);
    public float Passed => (float)(Time - _timeBegin);
    private double Time 
    { 
        get 
        {
            if(_isPaused)
            {
                return _timePaused;
            }
            else if(_type == TimerType.Simulation)
            {
                return SimulationOld.GameAge;
            }
            else if (_type == TimerType.Scaled)
            {
                return UnityEngine.Time.time;
            }
            return UnityEngine.Time.unscaledTime;
        } 
    }

   [SerializeField] private TimerType _type;
   [SerializeField] private bool _isPaused;
   [SerializeField] private float _duration;
   [SerializeField] private double _timeBegin;
   [SerializeField] private double _timeEnd;
   [SerializeField] private double _timePaused;

    public Timer(TimerType type = TimerType.Simulation)
    {
        _type = type;
    }
    public void Set(float duration)
    {
        _duration = duration;
        _timeBegin = Time;
        _timeEnd = Time + duration;
    }
    public void ForwardTime(float forward)
    {
        _timeBegin -= forward;
        _timeEnd -= forward;
        _timePaused -= forward;
    }
    public void Pause()
    {
        if (_isPaused)
            return;

        _timePaused = Time;                       // (!) BEFORE _isPaused = true

        _isPaused = true;
    }
    public void Unpause()
    {
        if (!_isPaused)
            return;

        _isPaused = false;

        var passed = Time - _timePaused;           // (!) AFTER _isPaused = false

        _timeBegin += passed;
        _timeEnd += passed;
    }
}

