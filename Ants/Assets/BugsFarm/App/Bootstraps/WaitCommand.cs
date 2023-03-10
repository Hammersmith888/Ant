using System;
using BugsFarm.Services.BootstrapService;
using UniRx;

namespace BugsFarm.App
{
    public class WaitCommand : Command
    {
        private readonly float _waitSeconds;
        public WaitCommand(float waitSeconds = 1f)
        {
            _waitSeconds = waitSeconds;
        }

        public override void Do()
        {
            Observable.Timer(TimeSpan.FromSeconds(_waitSeconds)).Subscribe(_ => OnDone());
        }
    }
}