using System;
using BugsFarm.Services.BootstrapService;
using UniRx;
using UnityEngine;
using Zenject;

namespace BugsFarm.App
{
    public class WaitContextInstallCommand : Command
    {
        private readonly string _contextName;
        private SceneContext _targetContext;
        private IDisposable _updateTask;
        private const float _intervalSeconds = 0.2f;

        public WaitContextInstallCommand(string contextName)
        {
            _contextName = contextName;
        }

        public override void Do()
        {
            if (_updateTask != null)
            {
                return;
            }

            _updateTask = Observable.Interval(TimeSpan.FromSeconds(_intervalSeconds))
                .Subscribe(_ => CheckContextInstalled());
        }

        private void CheckContextInstalled()
        {
            if (!_targetContext)
            {
                var contextObject = GameObject.Find(_contextName);
                if (!contextObject) return;

                _targetContext = contextObject.GetComponent<SceneContext>();
                if (!_targetContext) return;
            }

            if (!_targetContext.HasInstalled) return;
            _updateTask?.Dispose();
            _updateTask = null;
            OnDone();
        }
    }
}