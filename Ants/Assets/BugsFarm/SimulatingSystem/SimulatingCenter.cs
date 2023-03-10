using System;
using BugsFarm.Services.SaveManagerService;
using BugsFarm.SimulationSystem;
using UnityEngine;
using Zenject;


namespace BugsFarm.SimulatingSystem
{
    public class SimulatingCenter : ISavable
    {
        public static bool IsSimulating => _isSimulating;
        
        private ISimulationProcess _simulationProcess;
        private readonly ISimulationSystem _simulationSystem;
        private readonly IInstantiator _instantiator;

        private double _exitTime;
        private double _focusedTime;
        private double _currentSimulatingTime;
        private int _totalSimulatingTime;
        private static bool _isSimulating;

        private const int _timeDivider = 86400;

        public SimulatingCenter(IInstantiator instantiator, ISavableStorage savableStorage, ISimulationSystem simulationSystem)
        {
            _instantiator = instantiator;
            _simulationSystem = simulationSystem;
            savableStorage.Register(this);
            Application.focusChanged += SaveTimeOnLostFocus;
        }
        
        private void SaveTimeOnLostFocus(bool isFocused)
        {
            if (!isFocused)
            {
                _focusedTime = Tools.UtcNow();
            }
        }
        public double GetFocusDelta()
        {
            return Tools.UtcNow() - _focusedTime;
        }

        public string GetTypeKey()
        {
            return GetType().Name;
        }

        public string Save()
        {
            return JsonUtility.ToJson(new SimulatingData()
            {
                SimulatingTime = _totalSimulatingTime,
                ExitTime = Tools.UtcNow()
            });
        }

        public void Load(string jsonData)
        {
            var save = JsonUtility.FromJson<SimulatingData>(jsonData);
            _totalSimulatingTime = save.SimulatingTime;
            _exitTime = save.ExitTime;
        }

        public void SetSimulationTime(int timeInSeconds)
        {
            _totalSimulatingTime = timeInSeconds;
        }
        
        public void Simulate()
        {
            if (_totalSimulatingTime == 0)
            {
                _currentSimulatingTime= Math.Abs(Tools.UtcNow() - _exitTime);
            }
            else
            {
                _currentSimulatingTime = _totalSimulatingTime;
                _totalSimulatingTime = 0;
            }
            StartSimulation(_currentSimulatingTime);
        }

        private void StartSimulation(double totalSimulatingTime)
        {
            _isSimulating = true;
            _simulationProcess = _instantiator.Instantiate<SimulatingProcess>();
            float cycleTimes = (float) (totalSimulatingTime / _timeDivider);

            for (float i = 0; i < cycleTimes; i++)
            {
                float dayModifierFromCycleDuration = Mathf.Clamp01(cycleTimes - i);
                _simulationProcess.Simulate(_timeDivider / 60.0f * dayModifierFromCycleDuration, dayModifierFromCycleDuration, i + 1);
            }
            _simulationProcess.SimulateOneTime(_currentSimulatingTime, _simulationSystem.GameAge);
        }

        public void PostSimulate()
        {
            _simulationProcess?.PostSimulate(_currentSimulatingTime, _simulationSystem.GameAge);
            _simulationSystem.GameAge += _currentSimulatingTime;
            _simulationProcess?.Dispose();
            _isSimulating = false;
        }

        public void Dispose()
        {
            Application.focusChanged -= SaveTimeOnLostFocus;
        }


    }
}
