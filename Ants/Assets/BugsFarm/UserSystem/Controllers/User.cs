using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.CurrencySystem;
using BugsFarm.Services.SaveManagerService;
using BugsFarm.Services.StatsService;
using UniRx;
using UnityEngine;

namespace BugsFarm.UserSystem
{
    public class User : IUserInternal
    {
        public string Id => Dto.Id;
        public UserDto Dto { get; private set; }
        void IUserInternal.InitializeInternal(UserDto userDto)
        {
            Dto = userDto;
        }

        private readonly StatsCollectionStorage _statCollectionStorage;
        private const string _statLevelKey = "stat_level";
        private const string _statCurrencyKey = "stat_currency";

        public User(StatsCollectionStorage statCollectionStorage)
        {
            _statCollectionStorage = statCollectionStorage;
        }
        
        private StatModifiable _levelStat;
        private StatsCollection _statsCollection;
        public void Initialize()
        {
            _statsCollection = _statCollectionStorage.Get(Id);
            _levelStat = _statsCollection.Get<StatModifiable>(_statLevelKey);
            _levelStat.OnValueChanged += OnLevelValueChanged;
        }
        
        public int GetLevel()
        {
            if (NotInitialized())
            {
                return -1;
            }
            
            return (int) _levelStat.Value;
        }

        public void AddCurrency(string currencyId, int value)
        {
            if (NotInitialized())
            {
                return;
            }
            
            var statId = _statCurrencyKey + currencyId;
            if (!_statsCollection.HasEntity(statId))
            {
                throw new ArgumentException($"CurrencyID : {currencyId} does not exist");
            }

            var stat = _statsCollection.Get<StatVital>(statId);
            stat.CurrentValue += value;
            MessageBroker.Default.Publish(new UserCurrencyChangedProtocol(new CurrencyModel
            {
                Count = (int)stat.CurrentValue, 
                ModelID = currencyId
            }));
        }
        
        public int GetCurrency(string currencyId)
        {
            if (NotInitialized())
            {
                return default;
            }
            
            if (string.IsNullOrEmpty(currencyId))
            {
                throw new ArgumentException("CurrencyID is empty");
            }

            var statId = _statCurrencyKey + currencyId;
            if (!_statsCollection.HasEntity(statId))
            {
                throw new ArgumentException("CurrencyID is does not exist");
            }
            
            return (int)_statsCollection.GetVitalValue(statId);
        }

        public bool HasCurrency(CurrencyModel price)
        {
            var statId = _statCurrencyKey + price.ModelID;
            if (!_statsCollection.HasEntity(statId))
            {
                return false;
            }

            if (price.Count <= 0)
            {
                return true;
            }
            
            return _statsCollection.GetVitalValue(statId) >= price.Count;
        }

        public IEnumerable<CurrencyModel> GetCurrency()
        {
            if (NotInitialized())
            {
                return default;
            }
            return _statsCollection.GetAllStats()
                .Where(stat => stat.Id.Contains(_statCurrencyKey))
                .Select(stat => new CurrencyModel
                {
                    Count = (int)_statsCollection.GetVitalValue(stat.Id),
                    ModelID = stat.Id.Last().ToString()
                });
        }

        private bool NotInitialized()
        {
            if (Dto != null)
            {
                return false;
            }
            
            Debug.LogError("User not initialized");
            return true;
        }
        
        private void OnLevelValueChanged(object sender, EventArgs e)
        {
            MessageBroker.Default.Publish(new UserLevelChangedProtocol(GetLevel()));
        }
    }
}