using System;
using BugsFarm.Services.SaveManagerService;
using BugsFarm.UserSystem;
using BugsFarm.Utility;
using UniRx;
using UnityEngine;
using Zenject;

namespace BugsFarm.App
{
    public class AppState : IDisposable, IInitializable
    {
        private readonly IUser _user;
        private readonly ISaveManager _saveManager;
        private const float _autoSaveIntervalSecond = 30f;
        private IDisposable _autoSaveTask;

        private AppState(IUser user, ISaveManager saveManager)
        {
            _user = user;
            _saveManager = saveManager;
        }

        public void Initialize()
        {
            Application.quitting += OnApplicatiuQuiting;
            Application.focusChanged += OnApplicationFocus;
            InitAutoSave();
        }

        private void InitAutoSave()
        {
            if (_autoSaveTask != null)
            {
                return;
            }

            _autoSaveTask = Observable.Interval(TimeSpan.FromSeconds(_autoSaveIntervalSecond))
                .Subscribe(_ => OnAutoSaveTimerEnd());
        }

        public void Dispose()
        {
            Application.quitting -= OnApplicatiuQuiting;
            Application.focusChanged -= OnApplicationFocus;
            _autoSaveTask.Dispose();
        }

        private void OnAutoSaveTimerEnd()
        {
            Save();
        }

        private void OnApplicatiuQuiting()
        {
            Save();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                Debug.Log("Unfocused");
                Save();
            }
            else
            {
                Debug.Log("Focused");
            }

            //todo : else _simulationSystem.SimulateFrom();
        }

        private void Save()
        {
            _saveManager.Save(JsonUtility.ToJson(_user.Dto), PathConstants.GetUserPath(_user.Id));
            _saveManager.SaveAll(PathConstants.GetUserDataPath(_user.Id));
            Debug.Log($"{this} : Saved User ID : {_user.Id} ");
        }
    }
}