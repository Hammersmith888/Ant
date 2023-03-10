using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsFarm.BuildingSystem
{
    public class PointsController
    {
        public int FreePoints => MaxCount - BusyCount;
        public int MaxCount { get; private set; }
        public int BusyCount { get; private set; }

        private Queue<IPosSide> _taskPoints;
        private bool _initialized;

        public void Initialize(int maxCount, IEnumerable<IPosSide> taskPoints)
        {
            BusyCount = 0;
            MaxCount = maxCount;
            if (taskPoints == null)
            {
                throw new ArgumentException("Points is null");
            }
            _taskPoints = new Queue<IPosSide>(Tools.Shuffle_FisherYates(taskPoints));
            if (_taskPoints.Count == 0)
            {
                throw new InvalidOperationException("Points cannot be less than 1");
            }
            _initialized = true;
        }

        public void Replace(IEnumerable<IPosSide> taskPoints)
        {
            if (!_initialized)
            {
                Debug.LogError($"{this} : Not initialized");
                return;
            }
            if (taskPoints == null)
            {
                throw new ArgumentException("Points is null");
            }
            _taskPoints = new Queue<IPosSide>(Tools.Shuffle_FisherYates(taskPoints));
            if (_taskPoints.Count == 0)
            {
                throw new InvalidOperationException("Points cannot be less than 1");
            }
        }

        public void Dispose()
        {
            if (!_initialized)
            {
                Debug.LogError($"{this} : Not initialized");
                return;
            }

            _taskPoints = null;
            BusyCount = 0;
            MaxCount = 0;
            _initialized = false;
        }

        public bool HasPoint()
        {
            if (!_initialized)
            {
                Debug.LogError($"{this} : Not initialized");
                return false;
            }

            return FreePoints > 0;
        }

        public IPosSide GetPoint()
        {
            if (!_initialized)
            {
                Debug.LogError($"{this} : Not initialized");
                return default;
            }

            if (HasPoint())
            {
                var point = _taskPoints.Dequeue();
                _taskPoints.Enqueue(point);
                BusyCount++;
                return point;
            }

            return default;
        }

        public void FreePoint()
        {
            if (!_initialized)
            {
                Debug.LogError($"{this} : Not initialized");
                return;
            }

            if (BusyCount <= 0)
            {
                throw new InvalidOperationException();
            }

            BusyCount--;
        }

        public void Reset()
        {
            if (!_initialized)
            {
                Debug.LogError($"{this} : Not initialized");
                return;
            }

            BusyCount = 0;
        }
    }
}