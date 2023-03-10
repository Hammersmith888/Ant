using System;
using UnityEngine;

namespace BugsFarm.BuildingSystem
{
    [Serializable]
    public class HospitalSlotParam
    {
        [SerializeField] private string _id;
        [SerializeField] private float _currentValue;
        [SerializeField] private float _maxValue;
        [field: NonSerialized] public event Action<string> OnBaseValueChanged;
        [field: NonSerialized] public event Action<string> OnCurrentValueChanged;

        public string Id
        {
            get => _id;
            set => _id = value;
        }
        public float CurrentValue
        {
            get => _currentValue;
            set
            {
                if (_currentValue != value)
                {
                    if (_maxValue > 0)
                    {
                        _currentValue = Mathf.Clamp(value, 0, _maxValue);
                    }
                    else
                    {
                        _currentValue = Mathf.Max(0, value);
                    }

                    OnCurrentValueChanged?.Invoke(_id);
                }
            }
        }

        public float MaxValue
        {
            get => _maxValue;
            set
            {
                if (_maxValue != value)
                {
                    _maxValue = Mathf.Max(0, value);
                    OnBaseValueChanged?.Invoke(_id);
                }
            }
        }

        public void Dispose()
        {
            OnBaseValueChanged = null;
            OnCurrentValueChanged = null;
        }
    }
}