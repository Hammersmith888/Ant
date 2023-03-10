using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.Services.MonoPoolService;
using BugsFarm.Services.UIService;
using BugsFarm.UI;
using DG.Tweening;
using UnityEngine;

namespace BugsFarm.CurrencyCollectorSystem
{
    public class CurrencyCollectorSystem : ICurrencyCollectorSystem
    {
        private readonly ICurrencyAnimation _animation;
        private readonly IUIService _uiService;
        private readonly UIRoot _uiRoot;
        private readonly IconLoader _iconLoader;
        private readonly IMonoPool _monoPool;
        
        private Dictionary<string, ICurrencyView>_targets;
        private const float _spawnDelay = 0.1f;
        private const int _maxPoolCurrency = 15;
        private const int _maxCurrencyDecriment = 200;
        public CurrencyCollectorSystem(ICurrencyAnimation animation,
                                       IUIService uiService,
                                       UIRoot uiRoot,
                                       IconLoader iconLoader,
                                       IMonoPool monoPool)
        {
            _animation = animation;
            _uiService = uiService;
            _uiRoot = uiRoot;
            _iconLoader = iconLoader;
            _monoPool = monoPool;
        }

        public void FlushCollect()
        {
            Debug.LogWarning($"{this} : need flush collect coins before user leave the game");
        }
        
        public void Collect(Vector2 startPosition, 
                            string currencyID, 
                            int totalCount, 
                            bool useWorldPosition,
                            Action<int> onLeftCount = null,
                            Action onCompelte = null)
        {
            if (_targets == null)
            {
                var window = _uiService.Get<UIHeaderWindow>();
                _targets = window.CurrencyItems.ToDictionary(x=>x.CurrencyID);
            }
            if (totalCount <= 0 || !_targets.ContainsKey(currencyID))
                return;

            var targetPosition = _targets[currencyID].Target.position;

            float count01;
            if (totalCount <= _maxPoolCurrency)
            {
                count01 = totalCount / (float)_maxPoolCurrency;
            }
            else
            {
                count01 = Mathf.InverseLerp(1, _maxCurrencyDecriment, totalCount);
            }

            var count = Mathf.RoundToInt(Mathf.Lerp(1, _maxPoolCurrency, count01));
            var prev = 0;
            var totalLeft = totalCount;

            for (var i = 0; i < count; i++)
            {
                var cur = totalCount * (i + 1) / count;

                var coinValue = cur - prev;
                var currTotal = (totalLeft -= coinValue);
                var delay = _spawnDelay * i;
                var isLast = i == count - 1;

                DOVirtual.DelayedCall(delay, () =>
                {
                    var currencyItem = _monoPool.Spawn<CurrencyCollectorItem>(_uiRoot.PoolContainer);
                    var callBack = isLast ? onCompelte : () => onLeftCount?.Invoke(currTotal);
                    currencyItem.SetSprite(_iconLoader.LoadCurrency(currencyID));
                    _animation.Animate(startPosition, targetPosition, useWorldPosition, currencyItem,_uiRoot.UICamera, () => OnAnimationComplete(currencyItem, callBack));
                });
                prev = cur;
            }
        }

        private void OnAnimationComplete(IMonoPoolable item, Action onComplete)
        {
            //TODO : Make new user wallet system
            //GameResources.AddResource(currency, value);
            _monoPool?.Despawn(item);
            onComplete?.Invoke();
        }
    }
}