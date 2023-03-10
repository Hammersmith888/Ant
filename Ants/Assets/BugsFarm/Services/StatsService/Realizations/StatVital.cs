using System;
using UnityEngine;
using Zenject;

namespace BugsFarm.Services.StatsService
{ 
    public class StatVital : StatAttribute
    {
        public event EventHandler OnCurrentValueChanged;
        private StatVitalDto _statDto;
        
        [Inject]
        public void Inject()
        {
            _statDto = (StatVitalDto)StatDto;
        }
        
        public float CurrentValue
        {
            get => _statDto.CurrentValue;
            set
            {
                if (Math.Abs(_statDto.CurrentValue - value) > 0)
                {
                    _statDto.CurrentValue = Value > 0 ? Mathf.Clamp(value, 0 , Value) : Mathf.Max(0, value);
                    OnCurrentValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public void SetMax()
        {
            CurrentValue = Value;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            _statDto = null;
            OnCurrentValueChanged = null;
        }
    }
}