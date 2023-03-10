using System;
using BugsFarm.Game;
using UnityEngine;
using Zenject;

namespace BugsFarm.SimulationSystem.Obsolete
{
    public enum SimulationType
    {
        None,
        Accurate,
        Raw,
    }

    public class SimulationOld : MB_Singleton<SimulationOld>
    {
        public static bool Any => !Type.IsNullOrDefault();
        public static bool Raw => Type == SimulationType.Raw;
        public static SimulationType Type { get; private set; }
        public static float DeltaTime { get; private set; }
        public static double GameAge { get; private set; }

        [SerializeField] bool _simulationAtStart;
        //private DayNight _dayNight;
        //[Inject]
        //public void Inject(DayNight dayNight)
        //{
        //    _dayNight = dayNight;
        //}
        public void ResetGameAge()
        {
            GameAge = 0;
        }
        public void Restore()
        {
            Type = SimulationType.None;
            SetRealDeltaTime();
        }
        public void SetGameAge(double exitGameAge)
        {
            GameAge = exitGameAge;
        }
        public void SimulateFrom(double exitTime, bool showStats = false)
        {
            if (!_simulationAtStart)
            {
                GameEvents.OnSimulationEnd?.Invoke();
                return;
            }

            var passed = Tools.UtcNow() - exitTime;

            if (showStats)
            {
                var ts = TimeSpan.FromSeconds(passed);
                //UI_Control.Instance.Open(PanelID.Info);
                Panel_Info.Instance.SetText($"Passed: {ts.Days} days, {ts.Hours:D2}h {ts.Minutes:D2}m {ts.Seconds:D2}s");
            }
        
            Simulate(passed);
        }

        public void SimulationDemo(float hours) // DevMenu
        {
            double simulationTime = hours * 60 * 60;

            Simulate(simulationTime);
        }
        public static double GetAge(double timeBorn)
        {
            return GameAge - timeBorn;
        }

        private void SetRealDeltaTime()
        {
            DeltaTime = Time.deltaTime;
        }
        private void Update()
        {
            SetRealDeltaTime();
        
            UpdateObjects();
        }
        private void LateUpdate()
        {
            GameAge += Time.deltaTime;
        }
        private void UpdateObjects()
        {
            foreach (var objs in Keeper.Objects)
            {
                foreach (APlaceable placeable in objs.Value)
                {
                    placeable.Update();
                }
            }
        }
        private void UpdateAll()
        {
            // Maggots
            for (int i = Keeper.Maggots.Count - 1; i >= 0; i--) // no foreach, there can be Remove() calls
                Keeper.Maggots[i].Update();

            // Ants
            for (int i = Keeper.Ants.Count - 1; i >= 0; i--) // no foreach, there can be Remove() calls
                Keeper.Ants[i].Update();

            // Objects
            UpdateObjects();

            // DayPart
            //_dayNight.Update();
        }
        private void Iteration(float dt)
        {
            DeltaTime = dt;
            GameAge += dt;
            UpdateAll();
        }
        private void Simulate(double simulationTime, bool showStats = false)
        {
            var accurate = Consume(ref simulationTime, 60);
            var raw = Consume(ref simulationTime, Mathf.Infinity);

            var stats1 = Simulate(raw, 60, SimulationType.Raw);
            if (raw > 0)
                GameEvents.OnRawSimulationEnd?.Invoke();
            var stats2 = Simulate(accurate, 1, SimulationType.Accurate);


            Restore();
            //menu_Quests.Instance.Refresh();
            GameEvents.OnSimulationEnd?.Invoke();

            if (showStats)
            {
                int iterations = stats1.iterations + stats2.iterations;
                float realTime = stats1.time + stats2.time;

                //UI_Control.Instance.Open(PanelID.Info);
                Panel_Info.Instance.SetText($"Iterations: { iterations }, time: { realTime:F5} sec.");

                // Panel_Info.Instance.SetText( $"raw: { stats1.iterations}, { stats1.time :F5}, accurate: { stats2.iterations}, { stats2.time }" );
            }
        }
        private (int iterations, float time) Simulate(double simulationTime, double chunk, SimulationType simulation)
        {
            if (simulationTime <= 0)
                return (0, 0);


            Type = simulation;

            double rest = simulationTime;
            int iterations = 0;
            float rt_0 = Time.realtimeSinceStartup;

            while (rest > 0)
            {
                float dt = (float)System.Math.Min(chunk, rest);

                Iteration(dt);

                rest -= chunk;
                iterations++;
            }
            float rt_1 = Time.realtimeSinceStartup;

            return (iterations, rt_1 - rt_0);
        }
        private double Consume(ref double value, double max)
        {
            double result;

            if (value > max)
            {
                value -= max;
                result = max;
            }
            else
            {
                result = value;
                value = 0;
            }

            return result;
        }
    }
}