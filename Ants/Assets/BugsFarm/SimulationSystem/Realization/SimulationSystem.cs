using System;
using System.Threading.Tasks;
using BugsFarm.Services.SaveManagerService;
using UnityEngine;
using Zenject;

namespace BugsFarm.SimulationSystem
{
    public class SimulationSystem : ILateTickable, ISavable, ISimulationSystem
    {
        [Serializable]
        private struct SimulationData
        {
            public double GameAge;
            public double LastExitTime;
        }
        public event Action OnSimulationStart;
        public event Action OnSimulationEnd;
        public double GameAge { get; set; }
        public double LastExitTime { get; private set; }

        public float DeltaTime => Simulation ? _simulatedDeltaTime : OrigDeltaTime;
        public float OrigDeltaTime => Time.deltaTime;
        public bool Simulation { get; private set; }

        private readonly ITickableManagerInternal _simulationTickableManager;
        private float _simulatedDeltaTime;

        public SimulationSystem(ITickableManagerInternal simulationTickableManager,
                                ISavableStorage savableStorage)
        {
            savableStorage.Register(this);
            _simulationTickableManager = simulationTickableManager;
        }

        void ILateTickable.LateTick()
        {
            if (Simulation)
            {
                return;
            }
            GameAge += OrigDeltaTime;
        }

        public void Reset()
        {
            GameAge = 0;
            Simulation = false;
            _simulatedDeltaTime = OrigDeltaTime;
        }

        string ISavable.GetTypeKey()
        {
            return GetType().Name;
        }

        string ISavable.Save()
        {
            return JsonUtility.ToJson(new SimulationData
            {
                GameAge = GameAge,
                LastExitTime = Tools.UtcNow()
            });
        }

        void ISavable.Load(string jsonData)
        {
            var save = JsonUtility.FromJson<SimulationData>(jsonData);
            GameAge = save.GameAge;
            LastExitTime = save.LastExitTime;
        }

        public void Simulate(double simulationTime)
        {
            if (simulationTime <= 0)
            {
                return;
            }
            
            OnSimulationStart?.Invoke();
            Simulate(simulationTime, 60);
            OnSimulationEnd?.Invoke();
        }

        private void Simulate(double simulationTime, double chunk)
        {
            Simulation = true;
            while (simulationTime > 0 || !Simulation)
            {
                _simulatedDeltaTime = (float) Math.Min(chunk, simulationTime);
                GameAge += _simulatedDeltaTime;
                simulationTime -= chunk;
                _simulationTickableManager.Tick();
            }
            _simulatedDeltaTime = OrigDeltaTime;
            Simulation = false;
        }
    }
}