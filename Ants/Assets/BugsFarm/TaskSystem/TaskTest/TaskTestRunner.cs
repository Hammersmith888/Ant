using System.Threading;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace BugsFarm.TaskSystem
{
    public class TaskTestRunner : IInitializable
    {
        private readonly IInstantiator _instantiator;
        public TaskTestRunner(IInstantiator instantiator)
        {
            _instantiator = instantiator;
        }

        public void Initialize()
        {
            DOVirtual.DelayedCall(5f, () =>
            {
                for (var i = 0; i < 1000; i++)
                {
                    FakeUser();
                }
            });
        }

        private void FakeUser()
        {
            var task = _instantiator.Instantiate<TimerTaskTest>();
            task.OnComplete += _ => FakeUser();
            task.Execute(30);
        }
    }
}