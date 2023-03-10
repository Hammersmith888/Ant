using System;

namespace BugsFarm.Services.StatsService
{
    public abstract class StatModifier : IStatModifier
    {
        public event EventHandler OnValueChanged;
        public string Id { get; }
        public virtual int Order => int.MaxValue;
        
        public float Value
        {
            get => _value;
            set
            {
                if (Math.Abs(_value - value) <= 0.0000001f)
                {
                    return;
                }
                
                _value = value;
                OnValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool Stacks { get; }
        
        private float _value;
        public StatModifier(float value, 
                            bool stacks = true,
                            string modifierId = "")
        {
            Id = string.IsNullOrEmpty(modifierId) ? Guid.NewGuid().ToString() : modifierId;
            _value = value;
            Stacks = stacks;
        }

        public abstract float ApplyModifier(Stat stat, float modValue);
    }
}