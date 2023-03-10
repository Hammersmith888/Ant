using System;
using System.Linq;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.Locations;
using BugsFarm.Services.InteractorSystem;
using UniRx;
using UnityEngine;
using Zenject;

namespace BugsFarm.DayTimeSystem
{
    public class DayTimeInteractor : IInteractorService
    {
        public string Id => nameof(DayTimeInteractor);
        private readonly IInstantiator _instantiator;
        private readonly LocationSceneObject _locationSceneObject;
        private const float _nightTime = 22.99f;    // hours
        private const float _dayTime = 5.99f;       // hours
        private const float _dayWrappTime = 23.99f; // hours
        private NightProjector _nightProjector;
        private DayTime _lastUpdate = DayTime.None;
        private IDisposable _updateEvent;

        public DayTimeInteractor(IInstantiator instantiator, 
                                 LocationSceneObject locationSceneObject)
        {
            _instantiator = instantiator;
            _locationSceneObject = locationSceneObject;
        }

        public void Init()
        {
            Update();
        }

        public void Dispose()
        {
            _nightProjector?.Dispose();
            _nightProjector = null;
            _updateEvent?.Dispose();
            _updateEvent = null;
            _lastUpdate = DayTime.None;
        }

        private void Update()
        {
            var timeNow = GetTimeNow();
            var timeOfDay = (timeNow >= _dayTime && timeNow < _nightTime) ? DayTime.Day : DayTime.Night;
            var timerUpdate = 0f;
            if (_lastUpdate != timeOfDay)
            {
                switch (timeOfDay)
                {
                    case DayTime.Day:
                        _nightProjector?.Dispose();
                        _nightProjector = null;
                        timerUpdate = _nightTime - timeNow;
                        break;
                    
                    case DayTime.Night:
                        _nightProjector = _instantiator.Instantiate<NightProjector>();
                        _nightProjector.Initialize();
                        var wrappTime = timeNow >= _nightTime ? _dayWrappTime - timeNow : 0;
                        timerUpdate = (_dayTime - (timeNow - wrappTime)) + wrappTime;
                        break;
                }
                _locationSceneObject.SetBackground(LoadSprites(timeOfDay.ToString()));
            }
            
            _lastUpdate = timeOfDay;
            _updateEvent?.Dispose();
            MessageBroker.Default.Publish(new DayTimeChangedProtocol {TimeOfDay = timeOfDay});
            _updateEvent = Observable.Timer(TimeSpan.FromHours(timerUpdate)).Subscribe(_ => Update());
        }

        private float GetTimeNow()
        {
            var now = DateTime.Now;
            return now.Hour + (now.Minute + Format.SecondsToMinutes(now.Second)) / 60;
        }

        private Sprite[] LoadSprites(string timeKey)
        {
            var sprites = Resources.LoadAll<Sprite>($"Locations/{_locationSceneObject.Id}");
            return sprites.Where(x => x.name.Contains(timeKey)).ToArray();
        }
    }
}