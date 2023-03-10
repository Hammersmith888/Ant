using System;
using UnityEngine;

namespace BugsFarm.BuildingSystem
{
    [Serializable]
    public class OrderItemParam 
    {
        [SerializeField] private string _key;
        [SerializeField] private string _id;
        [SerializeField] private float _currentValue;
        [SerializeField] private float _baseValue;
        [field:NonSerialized] public event Action<OrderItemParam> OnBaseValueChanged;
        [field:NonSerialized] public event Action<OrderItemParam> OnCurrentValueChanged;
        
        public string Id 
        { 
            get => _id; 
            set => _id = value; 
        }
        public string Key 
        { 
            get => _key; 
            set => _key = value; 
        }

        public float CurrentValue
        {
            get => _currentValue;
            set
            {
                if (_currentValue != value)
                {
                    if (_baseValue > 0)
                    {
                        _currentValue = Mathf.Clamp(value, 0 , _baseValue);
                    }
                    else
                    {
                        _currentValue = Mathf.Max(0, value);
                    }

                    OnCurrentValueChanged?.Invoke(this);
                }
            }
        }
        
        public float BaseValue
        {
            get => _baseValue;
            set
            {
                if (_baseValue != value)
                {
                    _baseValue = Mathf.Max(0, value);
                    OnBaseValueChanged?.Invoke(this);
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