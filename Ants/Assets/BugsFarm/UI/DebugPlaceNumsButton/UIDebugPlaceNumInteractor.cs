using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.BuildingSystem;
using BugsFarm.Services.UIService;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Object = UnityEngine.Object;

namespace BugsFarm.UI
{
    public class UIDebugPlaceNumInteractor
    {
        private readonly IReservedPlaceSystem _reservedPlaceSystem;
        private readonly IInstantiator _instantiator;
        private readonly IUIService _uiService;
        private readonly UIRoot _uiRoot;
        private readonly UIWorldRoot _container;
        private readonly PlaceIdStorage _placeIdsStorage;
        private readonly Dictionary<string, Text> _placeIds;
        private IDisposable _updateEvent;
        private bool _showPlaceNums;
        private bool _initialized;

        public UIDebugPlaceNumInteractor(IReservedPlaceSystem reservedPlaceSystem,
                                       IInstantiator instantiator,
                                       IUIService uiService,
                                       UIRoot uiRoot,
                                       UIWorldRoot container,
                                       PlaceIdStorage placeIdsStorage)
        {
            _reservedPlaceSystem = reservedPlaceSystem;
            _instantiator = instantiator;
            _uiService = uiService;
            _uiRoot = uiRoot;
            _container = container;
            _placeIdsStorage = placeIdsStorage;
            _placeIds = new Dictionary<string, Text>();
        }

        public void Initialize()
        {
            if(_initialized) return;
            _initialized = true;
            var window = _uiService.Show<UIDebugPlaceNumsWindow>(_uiRoot.MiddleContainer);
            window.OnClick += OnWindowClick;
        }

        private void OnWindowClick()
        {
            if(!_initialized) return;
            _showPlaceNums = !_showPlaceNums;
            var window = _uiService.Show<UIDebugPlaceNumsWindow>(_uiRoot.MiddleContainer);
            window.SetTextColor(_showPlaceNums ? Color.green : window.OriginalColor);
            if (!_showPlaceNums)
            {
                Clear();
                _updateEvent?.Dispose();
                _updateEvent = null;
            }
            else
            {
                _updateEvent = Observable.Interval(TimeSpan.FromSeconds(.5f)).Subscribe(_ =>
                {
                    if (_showPlaceNums && _placeIds != null)
                    {
                        Clear();
                        foreach (var place in _reservedPlaceSystem.GetReservedPlaces())
                        {
                            var textObj = _instantiator.InstantiatePrefabForComponent<Text>(window.Prefab, _container.Transform);
                            textObj.text = place;
                            var palceId = _placeIdsStorage.Get(place);
                            textObj.transform.position = palceId.transform.position;
                            _placeIds.Add(place, textObj);
                        }
                    }
                });
            }
        }

        public void Dispose()
        {
            if(!_initialized) return;
            
            var window = _uiService.Show<UIDebugPlaceNumsWindow>(_uiRoot.MiddleContainer);
            _uiService.Hide<UIDebugPlaceNumsWindow>();
            window.SetTextColor(window.OriginalColor);
            
            _updateEvent?.Dispose();
            _updateEvent = null;          
            Clear();
            _initialized = false;
        }

        private void Clear()
        {
            if(!_initialized) return;
            foreach (var text in _placeIds.Values.ToArray())
            {
                if(!text) continue;
                Object.Destroy(text.gameObject);
            }

            _placeIds.Clear();
        }
    }
}