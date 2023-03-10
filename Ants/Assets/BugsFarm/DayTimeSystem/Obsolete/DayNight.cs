using System;
using BugsFarm.SimulationSystem.Obsolete;
using BugsFarm.UI;
using UnityEngine;

namespace BugsFarm.DayTimeSystem.Obsolete
{
    public enum DayPart
    {
        None,

        Day,
        DayToNight,
        Night,
        NightToDay
    }
    public class DayNight : MonoBehaviour, IPostSpawnInitable, IPostLoadRestorable
    {
        [Header("Settings")]
        [SerializeField] private int _dayBgn_h = 7;
        [SerializeField] private int _dayBgn_m = 0;
        [SerializeField] private int _dayEnd_h = 23;
        [SerializeField] private int _dayEnd_m = 0;
        [SerializeField] private int _trTime = 10;
        [SerializeField] private float _tint = 0.35f;

        [Header("Refs")]
        [SerializeField] private float _renderScale = 0.1f;
        [SerializeField] private Color _nightColor = new Color(1, 1, 1, .65f);
        [SerializeField] private SpriteSwitcher[] _nightSwitcher = null;

        public static DayPart DayPart { get; private set; }
        // private NightMaskSceneObject[] _nightRendererMasks = null;
        private Stats _stats => Keeper.Stats;
        private int _dayBgn;
        private int _dayEnd;

        public void HandleAspectRatio()
        {
            // _nightRendererMasks = FindObjectsOfType<NightMaskSceneObject>();
            // foreach (var item in _nightRendererMasks)
            // {
            //     item.Init(_nightColor);
            // } 
        }
        public void PostSpawnInit() => Set();
        public void PostLoadRestore() => Set();
        public void Update() => Set();
        public TimeSpan AdjustedTimeSpan() { return Adjusted().TimeOfDay; }
        public void SwitchDayTime()
        {
            float actual = Actual_sec();
            float addon;

            switch (DayPart)
            {
                case DayPart.Night:      addon = _dayBgn - actual; break;
                case DayPart.NightToDay: addon = _dayBgn + _trTime - actual; break;
                case DayPart.Day:        addon = _dayEnd - actual; break;
                case DayPart.DayToNight: addon = _dayEnd + _trTime - actual; break;

                default: addon = 0; break;
            }

            _stats.dayTimeAddon = addon;
        }
        public void SetIRL()
        {
            _stats.dayTimeAddon = (float)DateTime.Now.TimeOfDay.TotalSeconds - Actual_sec();
        }

        private DateTime Actual() { return _stats.gameStartUTC.Add(DateTime.Now - DateTime.UtcNow).AddSeconds(SimulationOld.GameAge); }
        private DateTime Adjusted() { return Actual().AddSeconds(_stats.dayTimeAddon); }
        private TimeSpan ActualTimeSpan() { return Actual().TimeOfDay; }
        private float Actual_sec() { return (float)ActualTimeSpan().TotalSeconds; }
        private float Adjusted_sec() { return (float)AdjustedTimeSpan().TotalSeconds; }
        private void Set()
        {
            _dayBgn = 60 * (60 * _dayBgn_h + _dayBgn_m);
            _dayEnd = 60 * (60 * _dayEnd_h + _dayEnd_m);


            float adjusted = Adjusted_sec();

            DayPart before = DayPart;

            if (adjusted < _dayBgn) DayPart = DayPart.Night;
            else if (adjusted < _dayBgn + _trTime) DayPart = DayPart.NightToDay;
            else if (adjusted < _dayEnd) DayPart = DayPart.Day;
            else if (adjusted < _dayEnd + _trTime) DayPart = DayPart.DayToNight;
            else DayPart = DayPart.Night;


            if (DayPart != before)
            {
                // if (DayPart != DayPart.Day)
                // {
                //     if (_nightRendererMasks != null)
                //     {
                //         foreach (var item in _nightRendererMasks)
                //         {
                //             item.Show();
                //         }
                //     }
                //
                //     if (_nightSwitcher != null)
                //     {
                //         foreach (var item in _nightSwitcher)
                //         {
                //             item.Switch(DayPart == DayPart.Day ? 0 : 1);
                //         }
                //     }
                // }
                // else
                // {
                //     foreach (var item in _nightRendererMasks)
                //     {
                //         item.Hide();
                //     }
                // }
            }


            float night_01 = 0;
            switch (DayPart)
            {
                case DayPart.Day:        night_01 = 0; break;
                case DayPart.Night:      night_01 = 1; break;
                case DayPart.DayToNight: night_01 = 0 + Mathf.InverseLerp(_dayEnd, _dayEnd + _trTime, adjusted); break;
                case DayPart.NightToDay: night_01 = 1 - Mathf.InverseLerp(_dayBgn, _dayBgn + _trTime, adjusted); break;
            }
        }
    }
}