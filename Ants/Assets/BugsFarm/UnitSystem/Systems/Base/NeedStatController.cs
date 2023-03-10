using System;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.Services.StatsService;
using BugsFarm.SimulationSystem;
using Zenject;
using TickableManager = Zenject.TickableManager;

namespace BugsFarm.UnitSystem
{
    public class NeedStatController : ITickable, INeedInfo
    {
        /// <summary>
        /// arg1 = Идентификатор сущности
        /// </summary>
        public event Action<string> OnNeed;

        public string Guid { get; private set; }
        public float Time { get; private set; }
        public string HeaderKey { get; private set; }
        public bool IsRestock { get; private set; }
        public bool IsNeed { get; private set; }
        public bool IsIdle { get; private set; } = true;
        public bool Disabled { get; private set; }
        public float NeedCount => _needCountStat.Value - _needCountStat.CurrentValue;

        private string _needPrefix;
        private const string _noNeed      = "NoNeed";
        private const string _alredyNeed  = "AlredyNeed";
        private const string _restockNeed = "Restock";
        private ITickableManager _tickableManager;
        private ISimulationSystem _simulationSystem;

        private StatVital _needCountStat; // сколько ресурса нужно
        private StatVital _noNeedTime;    // время когда не нужно
        private StatVital _needTime;      // время прошедшее с момента потребности

        [Inject]
        public void Inject(ITickableManager tickableManager,
                           ISimulationSystem simulationSystem,
                           StatsCollectionStorage statsCollectionStorage,
                           NeedStatPtotocol protocol)
        {
            _tickableManager = tickableManager;
            _simulationSystem = simulationSystem;
            _needPrefix = protocol.NeedPrefix;
            Guid = protocol.Guid;

            var statCollection = statsCollectionStorage.Get(protocol.Guid);
            _needCountStat = statCollection.Get<StatVital>(protocol.ResourceStatKey);
            _noNeedTime = statCollection.Get<StatVital>(protocol.NoNeedTimeStatKey);
            _needTime = statCollection.Get<StatVital>(protocol.NeedTimeStatKey);
            if (protocol.OnNeed != null)
            {
                OnNeed += protocol.OnNeed;  
            }
            _tickableManager.Add(this);
        }

        public void Tick()
        {
            if (Disabled) return;

            if (IsRestock) // Состояние пополнения ресурса
            {
                Time += _simulationSystem.DeltaTime;
                HeaderKey = _needPrefix + _restockNeed;
            }
            else // Состояния ресурса
            {
                if (IsIdle) // Нужда в ресурсе еще не наступила
                {
                    if (_noNeedTime.CurrentValue > 0)
                    {
                        _noNeedTime.CurrentValue -= Format.SecondsToMinutes(_simulationSystem.DeltaTime);
                        Time = Format.MinutesToSeconds(_noNeedTime.CurrentValue);
                        HeaderKey = _needPrefix + _noNeed;
                        IsNeed = false;
                        IsRestock = false;
                        if (_noNeedTime.CurrentValue <= 0)
                        {
                            _needTime.CurrentValue = 0;
                            _needCountStat.CurrentValue = 0; 
                        }
                    }
                    else
                    {
                        IsIdle = false;
                        IsNeed = true;
                        OnNeed?.Invoke(Guid);
                    }
                }
                else if (IsNeed) // нужда в ресурсе наступила, считаем время отсутствия ресурса
                {
                    _needTime.CurrentValue += Format.SecondsToMinutes(_simulationSystem.DeltaTime);
                    Time = Format.MinutesToSeconds(_needTime.CurrentValue);
                    HeaderKey = _needPrefix + _alredyNeed;
                }
            }
        }

        public void Dispose()
        {
            if (Disabled) return;
            Disabled = true;
            OnNeed = null;
            _needCountStat = null;
            _noNeedTime = null;
            _needTime = null;
            _tickableManager.Remove(this);
        }

        public void RestockStart()
        {
            if (IsRestock || Disabled) return;

            IsRestock = true;
            IsNeed = IsIdle = false;
            Time = 0;
        }

        public void RestockEnd()
        {
            if (!IsRestock || Disabled) return;
            IsRestock = false;
            IsNeed = NeedCount > 0;
            IsIdle = !IsNeed;
        }

        public void Update(float resourceDelta)
        {
            _needCountStat.CurrentValue += resourceDelta;
            var progers01 = _needCountStat.CurrentValue / _needCountStat.Value;
            _needTime.CurrentValue = progers01 > 0 ? 0 : _needTime.CurrentValue;
            _noNeedTime.CurrentValue = _noNeedTime.Value * progers01;
        }

        /// <summary>
        /// Если нужно начать с текущего доступного состояния
        /// </summary>
        public bool UseAvailable()
        {
            IsRestock = false;
            IsIdle = _noNeedTime.CurrentValue > 0;
            IsNeed = !IsIdle;

            _needCountStat.CurrentValue = IsIdle ? _needCountStat.CurrentValue : 0;
            _needTime.CurrentValue = IsNeed ? _needTime.CurrentValue : 0;
            return IsIdle;
        }
    }
}